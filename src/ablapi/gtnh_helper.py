"""Contains helpers for the /gtnh/* routes"""

import json
import re
from dataclasses import asdict, dataclass

from abllib import VolatileStorage, get_logger
from abllib.enum import Enum
from abllib.error import WrongTypeError
from flask import Response
from requests_cache import CachedSession

from ablapi.util import send_error

MODULE_NAME = "gtnh-helper"
logger = get_logger(MODULE_NAME)

class FetchResult(Enum):
    """All possible fetch results"""

    SUCCESS = 1
    NOT_FOUND = 2

@dataclass
class DAXXLVersion():
    """Represents all supported daily versions by this schema, with constraint start <= daily <= end"""

    id: int
    start: int
    end: int

DAXXL_VERSIONS: dict[int, DAXXLVersion] = {
    1: DAXXLVersion(1, 0, 500),
    2: DAXXLVersion(2, 501, 636),
    3: DAXXLVersion(3, 637, 9999),
}

@dataclass
class DownloadUrls():
    """Download urls for both client and server"""

    client: str
    server: str

@dataclass
class DailyVersion():
    """Represents a single daily release"""

    version: str
    run_number: int
    success: bool
    run_url: str
    run_url_html: str
    downloads: DownloadUrls | None

    def serialize(self) -> Response:
        """Return a json Response of the current object"""

        return Response(json.dumps(asdict(self)), mimetype="application/json")

@dataclass
class StableVersion():
    """Represents a single stable release"""

    version: str
    downloads: DownloadUrls

    def serialize(self) -> Response:
        """Return a json Response of the current object"""

        return Response(json.dumps(asdict(self)), mimetype="application/json")

def fetch_newest_daily() -> FetchResult:
    """Fetch data for newest daily version"""

    session: CachedSession = VolatileStorage["gtnh.session"]

    total_items = session.get(
        "https://api.github.com/repos/GTNewHorizons/DreamAssemblerXXL/actions/workflows/daily-modpack-build.yml/runs",
        params={"per_page": 1},
        timeout=10
    ).json()["total_count"]
    seen_items = 0

    page = 1
    while seen_items < total_items:
        runs = session.get(
            "https://api.github.com/repos/GTNewHorizons/DreamAssemblerXXL/actions/workflows/daily-modpack-build.yml/runs",
            params={"per_page": 100, "page": page},
            timeout=10
        ).json()
        current_run = runs["workflow_runs"][0]

        offset = 0
        while offset < len(runs["workflow_runs"]):
            current_run = runs["workflow_runs"][offset]
            version = _get_daxxl_version(current_run["run_number"])

            if current_run["status"] == "completed":
                success = current_run["conclusion"] == "success"
                run_struct = DailyVersion(
                    version=_fetch_latest_stable_version_str(),
                    run_number=current_run["run_number"],
                    success=success,
                    run_url=current_run["url"],
                    run_url_html=current_run["html_url"],
                    downloads=_get_daily_downloads(current_run["url"], version) if success else None
                )
                VolatileStorage["gtnh.daily.latest"] = run_struct
                VolatileStorage[f"gtnh.daily.{run_struct.run_number}"] = run_struct
                return FetchResult.SUCCESS

            offset += 1
            seen_items += 1

        page += 1

    logger.debug("Couldn't find any successful daily GTNH build")
    return FetchResult.NOT_FOUND

def fetch_specific_daily(target_daily: int) -> FetchResult:
    """Fetch data for newest daily version"""

    session: CachedSession = VolatileStorage["gtnh.session"]
    version = _get_daxxl_version(target_daily)

    total_items = session.get(
        "https://api.github.com/repos/GTNewHorizons/DreamAssemblerXXL/actions/workflows/daily-modpack-build.yml/runs",
        params={"per_page": 1},
        timeout=10
    ).json()["total_count"]
    seen_items = 0

    page = 1
    while seen_items < total_items:
        runs = session.get(
            "https://api.github.com/repos/GTNewHorizons/DreamAssemblerXXL/actions/workflows/daily-modpack-build.yml/runs",
            params={"per_page": 100, "page": page},
            timeout=10
        ).json()
        current_run = runs["workflow_runs"][0]

        offset = 0
        while offset < len(runs["workflow_runs"]):
            current_run = runs["workflow_runs"][offset]

            if current_run["run_number"] == target_daily:
                success = current_run["conclusion"] == "success"
                VolatileStorage[f"gtnh.daily.{target_daily}"] = DailyVersion(
                    version=_fetch_latest_stable_version_str(),
                    run_number=target_daily,
                    success=success,
                    run_url=current_run["url"],
                    run_url_html=current_run["html_url"],
                    downloads=_get_daily_downloads(current_run["url"], version) if success else None
                )
                return FetchResult.SUCCESS

            offset += 1
            seen_items += 1

        page += 1

    logger.debug(f"Couldn't find daily GTNH build '{target_daily}'")
    return FetchResult.NOT_FOUND

def fetch_newest_stable() -> FetchResult:
    """Fetch data for newest stable version"""

    session: CachedSession = VolatileStorage["gtnh.session"]

    releases_json = session.get(
        "https://www.gtnewhorizons.com/versions.json",
        timeout=10
    ).json()

    releases_keys = list(releases_json.keys())

    for release in releases_keys:
        match = re.search(r"(\d+\.\d+\.\d+)", release)
        if match is not None:
            VolatileStorage["gtnh.stable.latest"] = StableVersion(
                version=match.group(1),
                downloads=DownloadUrls(
                    client=releases_json[release]["mmc"]["java17_2XUrl"],
                    server=releases_json[release]["server"]["java17_2XUrl"]
                )
            )
            return FetchResult.SUCCESS

    logger.debug("Couldn't find any stable GTNH build, did the naming scheme change?")
    return FetchResult.NOT_FOUND

def fetch_specific_stable(target_stable: str) -> FetchResult:
    """Fetch data for specific stable version"""

    session: CachedSession = VolatileStorage["gtnh.session"]

    releases_json = session.get(
        "https://www.gtnewhorizons.com/versions.json",
        timeout=10
    ).json()

    for release in releases_json:
        if release == target_stable:
            VolatileStorage[f"gtnh.stable.{target_stable}"] = StableVersion(
                version=target_stable,
                downloads=DownloadUrls(
                    client=releases_json[release]["mmc"]["java17_2XUrl"],
                    server=releases_json[release]["server"]["java17_2XUrl"]
                )
            )
            return FetchResult.SUCCESS

    logger.debug("Couldn't find any stable GTNH build, did the naming scheme change?")
    return FetchResult.NOT_FOUND

def _fetch_latest_stable_version_str() -> str:
    session: CachedSession = VolatileStorage["gtnh.session"]

    releases = session.get(
        "https://api.github.com/repos/GTNewHorizons/GT-New-Horizons-Modpack/tags",
        params={"per_page": 10},
        timeout=10
    ).json()

    for release in releases:
        match = re.search(r"^(\d+\.\d+\.\d+)-", release["name"])
        if match is not None:
            return match.group(1)

    logger.debug("Couldn't find latest stable version str, falling back to latest stable release")
    if "gtnh.stable.latest" in VolatileStorage:
        stable_release: StableVersion = VolatileStorage["gtnh.stable.latest"]
        return stable_release.version

    send_error(MODULE_NAME, "Could not fetch latest stable version string from any source")
    raise RuntimeError()

def _notify_for_new_daily():
    pass

def _get_daily_client_download(run_url: str, version: DAXXLVersion) -> str:
    session: CachedSession = VolatileStorage["gtnh.session"]

    artifacts_json = session.get(
        run_url + "/artifacts",
        timeout=10
    ).json()
    for artifact in artifacts_json["artifacts"]:
        match version.id:
            case 1:
                if artifact["name"].endswith("mmcprism-new-java"):
                    return artifact["archive_download_url"]
            case 2:
                if artifact["name"].endswith("mmcprism-java17-25.zip"):
                    return artifact["archive_download_url"]
            case 3:
                if artifact["name"].endswith("mmcprism-java17-26.zip"):
                    return artifact["archive_download_url"]

    send_error(MODULE_NAME, f"Could not find client download url for run_url={run_url} version={version.id}")
    raise RuntimeError()

def _get_daily_server_download(run_url: str, version: DAXXLVersion) -> str:
    session: CachedSession = VolatileStorage["gtnh.session"]

    run_url += "/artifacts"

    artifacts_json = session.get(
        run_url,
        timeout=10
    ).json()
    for artifact in artifacts_json["artifacts"]:
        match version.id:
            case 1:
                if artifact["name"].endswith("server-new-java"):
                    return artifact["archive_download_url"]
            case 2:
                if artifact["name"].endswith("server-java17-25.zip"):
                    return artifact["archive_download_url"]
            case 3:
                if artifact["name"].endswith("server-java17-26.zip"):
                    return artifact["archive_download_url"]

    send_error(MODULE_NAME, f"Could not find server download url for run_url={run_url} version={version.id}")
    raise RuntimeError()

def _get_daily_downloads(run_url: str, version: DAXXLVersion) -> DownloadUrls:
    if not isinstance(run_url, str):
        raise WrongTypeError.with_values(run_url, str)
    if not isinstance(version, DAXXLVersion):
        raise WrongTypeError.with_values(version, DAXXLVersion)

    return DownloadUrls(
        client=_get_daily_client_download(run_url, version),
        server=_get_daily_server_download(run_url, version)
    )

def _get_daxxl_version(daily: int) -> DAXXLVersion:
    if not isinstance(daily, int):
        raise WrongTypeError.with_values(daily, int)

    for version in DAXXL_VERSIONS.values():
        if version.start <= daily <= version.end:
            return version

    send_error(MODULE_NAME, f"Could not find DAXXLVersion for daily {daily}")
    raise RuntimeError()

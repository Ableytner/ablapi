"""Contains helpers for the /gtnh/* routes"""

import re

from abllib import VolatileStorage, get_logger
from abllib.enum import Enum

logger = get_logger("gtnh-helper")

class FetchResult(Enum):
    """All possible fetch results"""

    SUCCESS = 1
    NOT_FOUND = 2

def fetch_newest_daily() -> FetchResult:
    """Fetch data for newest daily version"""

    session = VolatileStorage["gtnh.session"]

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

            if current_run["status"] == "completed":
                VolatileStorage["gtnh.daily.latest"] = {
                    "run_number": current_run["run_number"],
                    "conclusion": "success" if current_run["conclusion"] == "success" else "failure",
                    "url": current_run["url"],
                    "html_url": current_run["html_url"]
                }
                return FetchResult.SUCCESS

            offset += 1
            seen_items += 1

        page += 1

    logger.debug("Couldn't find any successful daily GTNH build")
    return FetchResult.NOT_FOUND

def fetch_specific_daily(target_daily: int) -> FetchResult:
    """Fetch data for newest daily version"""

    session = VolatileStorage["gtnh.session"]

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
                VolatileStorage[f"gtnh.daily.{target_daily}"] = {
                    "run_number": target_daily,
                    "conclusion": "success" if current_run["conclusion"] == "success" else "failure",
                    "url": current_run["url"],
                    "html_url": current_run["html_url"]
                }
                return FetchResult.SUCCESS

            offset += 1
            seen_items += 1

        page += 1

    logger.debug(f"Couldn't find daily GTNH build '{target_daily}'")
    return FetchResult.NOT_FOUND

def fetch_newest_stable() -> FetchResult:
    """Fetch data for newest stable version"""

    session = VolatileStorage["gtnh.session"]

    releases_json = session.get(
        "https://downloads.gtnewhorizons.com/versions.json",
        timeout=10
    ).json()["versions"]

    releases_keys = list(releases_json.keys())

    for release in releases_keys:
        match = re.search(r"(\d+\.\d+\.\d+)", release)
        if match is not None:
            VolatileStorage["gtnh.stable.latest"] = {
                "version": match.group(1),
                "download": {
                    "client": releases_json[release]["mmc"]["java17_2XUrl"],
                    "server": releases_json[release]["server"]["java17_2XUrl"]
                }
            }
            return FetchResult.SUCCESS

    logger.debug("Couldn't find any stable GTNH build, did the naming scheme change?")
    return FetchResult.NOT_FOUND

def fetch_specific_stable(target_stable: str) -> FetchResult:
    """Fetch data for specific stable version"""

    session = VolatileStorage["gtnh.session"]

    releases_json = session.get(
        "https://downloads.gtnewhorizons.com/versions.json",
        timeout=10
    ).json()["versions"]

    for release in releases_json:
        if release == target_stable:
            VolatileStorage[f"gtnh.stable.{target_stable}"] = {
                "version": target_stable,
                "download": {
                    "client": releases_json[release]["mmc"]["java17_2XUrl"],
                    "server": releases_json[release]["server"]["java17_2XUrl"]
                }
            }
            return FetchResult.SUCCESS

    logger.debug("Couldn't find any stable GTNH build, did the naming scheme change?")
    return FetchResult.NOT_FOUND

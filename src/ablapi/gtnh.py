"""Contains the /gtnh/* routes"""

import os
import re
from time import sleep

import requests
from abllib import VolatileStorage, get_logger
from abllib.error import KeyNotFoundError
from abllib.pproc import WorkerThread
from requests_cache import CachedSession
from schedule import Scheduler

import ablapi.gtnh_helper as helper
from ablapi.util import register_endpoint

logger = get_logger("gtnh")

if "GITHUB_TOKEN" not in os.environ:
    raise KeyNotFoundError("Environment variable 'GITHUB_TOKEN' isn't set")
VolatileStorage["gtnh.github_token"] = os.environ["GITHUB_TOKEN"]

headers: dict[str, str | bytes] = {
    "Accept": "application/vnd.github+json",
    "Accept-Charset": "UTF-8",
    "Authorization": f"Bearer {VolatileStorage['gtnh.github_token']}",
    "X-GitHub-Api-Version": "2022-11-28"
}

session = CachedSession("requests-cache/gtnh-github", expire_after=60 * 5)
session.headers = headers
VolatileStorage["gtnh.session"] = session

# test github token (doesn't actually test token, as it is no longer required here)
session_test_response = requests.get(
    "https://api.github.com/repos/GTNewHorizons/DreamAssemblerXXL/actions/workflows/daily-modpack-build.yml/runs",
    params={"per_page": 1},
    headers=headers,
    timeout=10
)
assert session_test_response.ok
assert "workflow_runs" in session_test_response.json()

def daily_version(version: str):
    """Endpoint for fetching a specific daily GTNH version"""

    if version == "latest":
        _wait_for_latest_daily(60)
        return VolatileStorage["gtnh.daily.latest"]

    try:
        version_int = int(version)
    except Exception:
        return f"Invalid version '{version}', needs to be a number", 400

    _wait_for_latest_daily(30)

    newest_daily_version = VolatileStorage["gtnh.daily.latest.run_number"]
    if version_int > newest_daily_version:
        return f"Invalid version '{version}', needs to be <= {newest_daily_version}", 400

    try:
        fetch_result = helper.fetch_specific_daily(version_int)
    except Exception:
        logger.exception(f"Error during search for daily version {version}")
        return "Internal error"

    match fetch_result:
        case helper.FetchResult.SUCCESS:
            return VolatileStorage[f"gtnh.daily.{version}"]
        case helper.FetchResult.NOT_FOUND:
            return f"Daily version '{version}' wasn't found"

    return "Internal error"

def stable_version(version: str):
    """Endpoint for fetching a specific stable GTNH version"""

    if version == "latest":
        return VolatileStorage["gtnh.stable.latest"]

    match = re.search(r"(\d+\.\d+\.\d+)", version)
    if match is None:
        return f"Invalid version '{version}', needs to be in format '<int>.<int>.<int>", 400

    try:
        fetch_result = helper.fetch_specific_stable(version)
    except Exception:
        logger.exception(f"Error during search for stable version {version}")
        return "Internal error"

    match fetch_result:
        case helper.FetchResult.SUCCESS:
            return VolatileStorage[f"gtnh.stable.{version}"]
        case helper.FetchResult.NOT_FOUND:
            return f"Stable version '{version}' wasn't found"

    return "Internal error"

def _wait_for_latest_daily(seconds: float) -> None:
    """Wait for at most `seconds` seconds until gtnh.daily.latest is set"""
    seconds_passed = 0.0

    while "gtnh.daily.latest" not in VolatileStorage and seconds_passed < seconds:
        sleep(0.1)
        seconds_passed += 0.1

    if seconds_passed >= seconds:
        raise TimeoutError("Timed out waiting for latest daily version fetch")

def info_getter_func():
    """Fetch all data this module needs in the background"""

    logger.info("GTNH info getter thread started!")

    scheduler = Scheduler()

    assert helper.fetch_newest_daily() == helper.FetchResult.SUCCESS
    scheduler.every().hour.at("13:00").do(helper.fetch_newest_daily)

    assert helper.fetch_newest_stable() == helper.FetchResult.SUCCESS
    scheduler.every().hour.at("13:00").do(helper.fetch_newest_stable)

    logger.info("GTNH info getter thread entering loop")

    while True:
        scheduler.run_pending()
        sleep(60)

register_endpoint("/gtnh/daily/<version>", daily_version)
register_endpoint("/gtnh/stable/<version>", stable_version)

info_getter_thread = WorkerThread(target=info_getter_func, daemon=True)
info_getter_thread.start()

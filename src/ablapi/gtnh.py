"""Contains the /gtnh/* routes"""

import os
import re
from time import sleep

from abllib import VolatileStorage, get_logger
from abllib.error import KeyNotFoundError
from abllib.pproc import WorkerThread
from requests import Session
from schedule import Scheduler

from ablapi.util import register_endpoint

logger = get_logger("gtnh")

if "github_token" not in os.environ:
    raise KeyNotFoundError("Environment variable 'github_token' isn't set")
VolatileStorage["gtnh.github_token"] = os.environ["github_token"]

session = Session()
session.headers = {
    "Accept": "application/vnd.github+json",
    "Accept-Charset": "UTF-8",
    "Authorization": f"Bearer {VolatileStorage['gtnh.github_token']}",
    "X-GitHub-Api-Version": "2022-11-28"
}

def latest_daily_version():
    """Endpoint for fetching the latest daily GTNH version"""

    return VolatileStorage["gtnh.daily.latest"]

def latest_stable_version():
    """Endpoint for fetching the latest stable GTNH version"""

    return VolatileStorage["gtnh.stable.latest"]

def info_getter_func():
    """Fetch all data this module needs in the background"""

    logger.info("GTNH info getter thread started!")

    scheduler = Scheduler()

    fetch_newest_daily()
    scheduler.every().hour.at("13:00").do(fetch_newest_daily)

    fetch_newest_stable()
    scheduler.every().hour.at("13:00").do(fetch_newest_stable)

    while True:
        scheduler.run_pending()
        sleep(60)

def fetch_newest_daily():
    """Fetch data for newest daily version"""

    runs = session.get(
        "https://api.github.com/repos/GTNewHorizons/DreamAssemblerXXL/actions/workflows/daily-modpack-build.yml/runs",
        params={"per_page": 100},
        timeout=60
    ).json()
    current_run = runs["workflow_runs"][0]

    offset = 0
    while offset < 100:
        current_run = runs["workflow_runs"][offset]

        if current_run["status"] == "completed":
            VolatileStorage["gtnh.daily.latest"] = {
                "run_number": current_run["run_number"],
                "conclusion": "success" if current_run["conclusion"] == "success" else "failure",
                "url": current_run["url"],
                "html_url": current_run["html_url"]
            }
            return

        offset += 1

    logger.error("Couldn't find any successful daily GTNH build")

def fetch_newest_stable():
    """Fetch data for newest stable version"""

    releases_text = session.get(
        "https://downloads.gtnewhorizons.com/ClientPacks/?raw",
        timeout=60
    ).text
    releases_list = list(
        filter(
            lambda item: "betas" not in item,
            releases_text.split("\n")
        )
    )
    releases_list.reverse()

    for release in releases_list:
        match = re.search(r"GT_New_Horizons_(\d+\.\d+\.\d+)_Client_Java", release)
        if match is not None:
            VolatileStorage["gtnh.stable.latest"] = {
                "version": match.group(1),
                "download_url": release
            }
            return

    logger.error("Couldn't find any stable GTNH build, did the naming scheme change?")

register_endpoint("/gtnh/latest/daily", latest_daily_version)
register_endpoint("/gtnh/latest/stable", latest_stable_version)

info_getter_thread = WorkerThread(target=info_getter_func, daemon=True)
info_getter_thread.start()

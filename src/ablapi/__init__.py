"""Imports all submodules"""

# pylint: disable=wrong-import-position
# ruff: noqa: E402, F401

import os

from abllib import log, storage
from abllib.log import LogLevel, get_logger

if "LOG_LEVEL" in os.environ:
    log_level = LogLevel.from_str(os.environ["LOG_LEVEL"].strip())
else:
    log_level = LogLevel.INFO

log.initialize(log_level)
log.add_console_handler()

# quiet down other loggers
get_logger("urllib3").setLevel(LogLevel.INFO.value)
get_logger("requests_cache").setLevel(LogLevel.INFO.value)

storage.initialize()

# setup requests cache dir
os.makedirs("requests-cache", exist_ok=True)

from ablapi.initialize import run

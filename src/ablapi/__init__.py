"""Imports all submodules"""

# pylint: disable=wrong-import-position
# ruff: noqa: E402

import os

from abllib import log, storage

if "log_level" in os.environ:
    log_level = log.LogLevel.from_str(os.environ["log_level"])
else:
    log_level = log.LogLevel.INFO

log.initialize(log_level)
log.add_console_handler()
log.add_file_handler("latest.log")

storage.initialize()

from ablapi import initialize, util

__exports__ = [
    initialize,
    util
]

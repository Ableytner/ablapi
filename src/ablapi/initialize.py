"""Initialize everything before the server starts"""

import os

from abllib import VolatileStorage, get_logger
from flask import Flask

from ablapi import util

logger = get_logger("initialize")

def run() -> Flask:
    """Run all setup functions."""

    util.print_separator_large()

    logger.info("starting setup")
    load_environment_variables()
    initialize_flask_app()
    logger.info("finished setup")

    util.load_modules()
    logger.info("finished creating Flask app")

    util.print_separator_large()

    return VolatileStorage["app"]

def load_environment_variables() -> None:
    """Load and store all environment variables"""


    VolatileStorage["modules_to_load"] = []
    if "modules" in os.environ:
        VolatileStorage["modules_to_load"] = os.environ["modules"].replace(", ", " ").replace(",", " ").split(" ")

def initialize_flask_app() -> None:
    """Initialize the flask app"""

    app = Flask("ablapi")

    VolatileStorage["app"] = app

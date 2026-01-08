"""Various utility functions"""

import fcntl
import importlib
import os
import struct
import sys
import termios
import traceback
from typing import Any, Callable

from abllib import CacheStorage, VolatileStorage, get_logger
from abllib.error import NotInitializedError, WrongTypeError
from flask import Flask

logger = get_logger("util")

def register_endpoint(location: str, callback: Callable[[], Any]) -> None:
    """Register a callback to a location"""

    if not isinstance(location, str):
        raise WrongTypeError.with_values(location, str)
    if not location.startswith("/") or location.endswith("/"):
        raise ValueError(f"Unexpected location format: {location}")
    if not callable(callable):
        raise WrongTypeError.with_values(callback, Callable[[], Any])

    traces = traceback.format_list(traceback.extract_stack())
    traces.reverse()
    module_line = traces[1].split("\n")[0].strip()
    module_filename = os.path.basename(module_line) \
                          .split("\"", maxsplit=1)[0] \
                          .split(".", maxsplit=1)[0]

    if f"endpoints.{module_filename}" not in VolatileStorage:
        VolatileStorage[f"endpoints.{module_filename}"] = []

    VolatileStorage[f"endpoints.{module_filename}"].append((
        location,
        callback
    ))

    logger.debug(f"Registered endpoint for {module_filename}: {location}")

def load_modules() -> None:
    """Load all registered modules"""

    app: Flask = VolatileStorage.get("app")
    if app is None:
        raise NotInitializedError("Flask app is not yet created")

    print_separator_large()
    logger.info("Starting to load modules")

    modules_to_load = []
    if "modules_to_load" in VolatileStorage:
        modules_to_load = VolatileStorage.pop("modules_to_load")

    modules_counter = 0
    endpoints_counter = 0
    for module_to_load in modules_to_load:
        print_separator_small()
        logger.info(f"Importing module: {module_to_load}")

        module_path = find_file(f"{module_to_load}.py", os.path.dirname(__file__))
        if module_path is None:
            raise ModuleNotFoundError(f"Can't find module {module_to_load}")

        module_path = module_path.replace(".py", "") \
                                 .replace("/", ".") \
                                 .replace("\\", ".")
        module = importlib.import_module(module_path)
        sys.modules[module_to_load] = module

        logger.info(f"Loading module: {module_to_load}")
        for location, callback in VolatileStorage.get(f"endpoints.{module_to_load}", []):
            logger.info(f"Loading endpoint: {location}")
            app.route(location)(callback)
            endpoints_counter += 1

        modules_counter += 1

    print_separator_small()
    logger.info(f"Loaded {modules_counter} module{'s' if modules_counter != 1 else ''} "
                f"with {endpoints_counter} endpoint{'s' if endpoints_counter != 1 else ''}")

def find_file(filename: str, base_dir: str) -> str | None:
    """
    Search for and return the first file whose name matches.

    Return the path starting from and including base_dir.

    https://stackoverflow.com/a/1724723
    """

    for root, _, files in os.walk(base_dir):
        if filename in files:
            return_path = os.path.join(root, filename)
            return_path = return_path[return_path.rindex(os.path.basename(base_dir)):]
            return return_path

    return None

def print_separator_huge():
    """Log a line of !!! """

    logger.info("!" * get_terminal_width_for_logging())

def print_separator_large():
    """Log a line of === """

    logger.info("=" * get_terminal_width_for_logging())

def print_separator_small():
    """Log a line of --- """

    logger.info("-" * get_terminal_width_for_logging())

def get_terminal_width() -> int:
    """
    Return the terminal width, or a default if detection failed.

    Caches the result, so only the first call is expensive.

    https://stackoverflow.com/a/3010495
    """

    if "terminal_width" in CacheStorage:
        return CacheStorage["terminal_width"]

    # pylint: disable-next=unused-variable
    h, w, hp, wp = struct.unpack(
        'HHHH',
        fcntl.ioctl(
            sys.stdout.fileno(),
            termios.TIOCGWINSZ,
            struct.pack('HHHH', 0, 0, 0, 0)
        )
    )
    w -= 1
    CacheStorage["terminal_width"] = w

    return w

def get_terminal_width_for_logging() -> int:
    """
    Return the terminal width remaining for logging text.

    Caches the result, so only the first call is expensive.
    """

    total_width = get_terminal_width()

    sample_output = "[2026-01-07 22:27:39] [INFO    ] util: "

    return total_width - len(sample_output)

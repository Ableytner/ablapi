"""Pytest fixtures"""

import pytest
from abllib import log

from ablapi import initialize

# pylint: disable=protected-access

logger = log.get_logger("test")

@pytest.fixture(scope="session", autouse=True)
def setup():
    """Setup everything"""

    initialize.initialize_flask_app()
    # doesn't actually run, just sets up environment
    initialize.run()

    yield None

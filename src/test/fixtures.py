"""Pytest fixtures"""

import pytest
from abllib import VolatileStorage, log

from ablapi import initialize

# pylint: disable=protected-access

logger = log.get_logger("test")

@pytest.fixture(scope="session", autouse=True)
def setup():
    """Setup everything"""

    initialize.run()

    yield None

@pytest.fixture()
def client():
    """Return a test client"""

    return VolatileStorage["app"].test_client()

@pytest.fixture()
def runner():
    """Return a test cli runner"""

    return VolatileStorage["app"].test_cli_runner()

"""Module containing tests for the healthz module"""

from abllib import log
from flask.testing import FlaskClient

logger = log.get_logger("pylint")

def test_healthz(client: FlaskClient):
    """Checks if /healthz is reachable"""

    result = client.get("/healthz")

    assert result.status_code == 200
    assert result.text == "OK"

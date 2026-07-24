"""Module containing tests for the gtnh module"""


from abllib import log
from flask.testing import FlaskClient

logger = log.get_logger("pylint")

def test_daily_specific_v1(client: FlaskClient):
    """Checks if fetching a specific daily version works"""

    result = client.get("/gtnh/daily/10").get_json(force=True)

    assert isinstance(result, dict)
    assert result["run_number"] == 10
    assert result["success"]
    assert result["run_url"] == "https://api.github.com/repos/GTNewHorizons/DreamAssemblerXXL/actions/runs/15650945850"
    assert result["run_url_html"] == "https://github.com/GTNewHorizons/DreamAssemblerXXL/actions/runs/15650945850"

def test_daily_specific_v2(client: FlaskClient):
    """Checks if fetching a specific daily version works"""

    result = client.get("/gtnh/daily/501").get_json(force=True)

    assert isinstance(result, dict)
    assert result["run_number"] == 501
    assert result["success"]
    assert result["run_url"] == "https://api.github.com/repos/GTNewHorizons/DreamAssemblerXXL/actions/runs/25462587801"
    assert result["run_url_html"] == "https://github.com/GTNewHorizons/DreamAssemblerXXL/actions/runs/25462587801"

def test_daily_specific_v3(client: FlaskClient):
    """Checks if fetching a specific daily version works"""

    result = client.get("/gtnh/daily/639").get_json(force=True)

    assert isinstance(result, dict)
    assert result["run_number"] == 639
    assert result["success"]
    assert result["run_url"] == "https://api.github.com/repos/GTNewHorizons/DreamAssemblerXXL/actions/runs/30048434960"
    assert result["run_url_html"] == "https://github.com/GTNewHorizons/DreamAssemblerXXL/actions/runs/30048434960"

def test_daily_failed(client: FlaskClient):
    """Checks if fetching a specific daily version works"""

    result = client.get("/gtnh/daily/500").get_json(force=True)

    assert isinstance(result, dict)
    assert result["run_number"] == 500
    assert not result["success"]
    assert result["run_url"] == "https://api.github.com/repos/GTNewHorizons/DreamAssemblerXXL/actions/runs/25460219483"
    assert result["run_url_html"] == "https://github.com/GTNewHorizons/DreamAssemblerXXL/actions/runs/25460219483"
    assert result["downloads"] is None

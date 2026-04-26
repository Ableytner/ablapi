"""Module containing tests for the gtnh module"""

from abllib import log

from ablapi import gtnh

logger = log.get_logger("pylint")

def test_get_specific_daily():
    """Checks if fetching a specific daily version works"""

    result = gtnh.daily_version("10")

    assert isinstance(result, dict)
    assert result["run_number"] == 10
    assert result["conclusion"] == "success"
    assert result["url"] == "https://api.github.com/repos/GTNewHorizons/DreamAssemblerXXL/actions/runs/15650945850"
    assert result["html_url"] == "https://github.com/GTNewHorizons/DreamAssemblerXXL/actions/runs/15650945850"

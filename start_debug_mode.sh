#!/bin/sh

. ./.env
log_level=debug modules=$modules github_token=$github_token venv/bin/python -m flask --app src/main:run run --debug --no-reload

#!/bin/sh

. ./.env
LOG_LEVEL=debug MODULES=$MODULES GITHUB_TOKEN=$GITHUB_TOKEN venv/bin/python -m flask --app src/main:run run --debug --no-reload

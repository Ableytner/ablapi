#!/usr/bin/env bash

# registers a spotify user for unittests

set -euo pipefail

if [[ $# -lt 2 ]]; then
    echo "Usage: $0 <userid> <token> [base_url]" >&2
    exit 1
fi

USERID="$1"
TOKEN="$2"
BASE="${3:-http://localhost:44331}"
BASE="${BASE%/}"

# 1) exchange userid + token for a JWT
JWT=$(curl -sf -X POST "$BASE/auth" \
    -H "Authorization: Basic $(printf '%s:%s' "$USERID" "$TOKEN" | base64 | tr -d '\n')" \
    | jq -r .token)

if [[ -z "$JWT" || "$JWT" == "null" ]]; then
    echo "Failed to obtain a JWT for user $USERID" >&2
    exit 1
fi

# 2) register for the everything playlist
curl -sS -X POST "$BASE/spotify/everything/register" \
    -H "Authorization: Bearer $JWT"
echo

#!/usr/bin/env bash
# Usage: ./verify_forwarded.sh https://your-host:443
BASE="$1"
if [ -z "$BASE" ]; then
  echo "Usage: $0 <base-url>  e.g. https://localhost:5001"
  exit 1
fi

echo "Calling $BASE/api/debug/ip WITHOUT X-Forwarded-For"
curl -sS "$BASE/api/debug/ip" | jq .

echo
echo "Calling $BASE/api/debug/ip WITH X-Forwarded-For=203.0.113.45 and X-Real-IP=203.0.113.45"
curl -sS -H "X-Forwarded-For: 203.0.113.45" -H "X-Real-IP: 203.0.113.45" "$BASE/api/debug/ip" | jq .

echo
echo "If Kestrel sees RemoteIp = 203.0.113.45 then Forwarded headers are applied correctly (and your KnownProxies/KnownNetworks must include the proxy IP)."
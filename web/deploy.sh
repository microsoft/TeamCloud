#!/bin/bash

exitWithMessageOnError () {
  if [ ! $? -eq 0 ]; then
    echo "An error has occurred during web site deployment."
    echo $1
    exit 1
  fi
}


# Verify node.js installed
hash node 2>/dev/null
exitWithMessageOnError "Missing node.js executable, please install node.js, if already installed make sure it can be reached from current environment."

KUDU_SYNC_CMD=${KUDU_SYNC_CMD//\"}


if [[ ! -n "$DEPLOYMENT_SOURCE" ]]; then
  echo "No DEPLOYMENT_SOURCE set."
  exit 1
fi

if [[ ! -n "$DEPLOYMENT_TARGET" ]]; then
  echo "No DEPLOYMENT_TARGET set."
  exit 1
fi


echo "Handling node.js deployment.\n"

if [ -e "$DEPLOYMENT_SOURCE/package.json" ]; then

  cd "$DEPLOYMENT_SOURCE"

  echo "Running npm install.\n"
  eval npm install
  exitWithMessageOnError "npm install failed."

  echo "Running npm run build\n"
  eval npm run build
  exitWithMessageOnError "npm run build failed."

  cd - > /dev/null
fi

echo "Running kudoSync.\n"

if [[ "$IN_PLACE_DEPLOYMENT" -ne "1" ]]; then
  "$KUDU_SYNC_CMD" -v 50 -f "$DEPLOYMENT_SOURCE/build" -t "$DEPLOYMENT_TARGET" -n "$NEXT_MANIFEST_PATH" -p "$PREVIOUS_MANIFEST_PATH" -i ".git;.hg;.deployment;deploy.sh"
  exitWithMessageOnError "Kudu Sync failed."
fi

echo "Finished successfully.\n"

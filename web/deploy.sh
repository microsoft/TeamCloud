#!/bin/bash

exitWithMessageOnError () {
  if [ ! $? -eq 0 ]; then
    echo " "
    echo "[$(date +"%Y-%m-%d-%H%M%S")] An error has occurred during web site deployment."
    echo "[$(date +"%Y-%m-%d-%H%M%S")] $1"
    exit 1
  fi
}

logMessage() {
  echo " "
  echo "[$(date +"%Y-%m-%d-%H%M%S")] $1"
  echo " "
}

# Verify node.js installed
hash node 2>/dev/null
exitWithMessageOnError "Missing node.js executable, please install node.js, if already installed make sure it can be reached from current environment."

KUDU_SYNC_CMD=${KUDU_SYNC_CMD//\"}


if [[ ! -n "$DEPLOYMENT_SOURCE" ]]; then
  logMessage "No DEPLOYMENT_SOURCE set."
  exit 1
fi

if [[ ! -n "$DEPLOYMENT_TARGET" ]]; then
  logMessage "No DEPLOYMENT_TARGET set."
  exit 1
fi


logMessage "Handling node.js deployment."

if [ -e "$DEPLOYMENT_SOURCE/package.json" ]; then

  cd "$DEPLOYMENT_SOURCE"

  logMessage "Running npm install."
  eval npm install
  exitWithMessageOnError "npm install failed."

  logMessage "Running npm run build"
  eval npm run build
  exitWithMessageOnError "npm run build failed."

  cd - > /dev/null
fi

logMessage "Running kudoSync."

if [[ "$IN_PLACE_DEPLOYMENT" -ne "1" ]]; then
  "$KUDU_SYNC_CMD" -v 50 -f "$DEPLOYMENT_SOURCE/build" -t "$DEPLOYMENT_TARGET" -n "$NEXT_MANIFEST_PATH" -p "$PREVIOUS_MANIFEST_PATH" -i ".git;.hg;.deployment;deploy.sh"
  exitWithMessageOnError "Kudu Sync failed."
fi


logMessage "Finished successfully."

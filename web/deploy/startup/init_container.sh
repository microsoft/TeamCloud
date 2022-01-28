#!/usr/bin/env bash

# Get environment variables to show up in SSH session
eval $(printenv | sed -n "s/^\([^=]\+\)=\(.*\)$/export \1=\2/p" | sed 's/"/\\\"/g' | sed '/=/s//="/' | sed 's/$/"/' >> /etc/profile)

pushd /home/site/wwwroot/static/js > /dev/null

    pattern="main.*.js"

    files=( $(compgen -W "$pattern") )
    mainFile=$files

    sed -i 's|__REACT_APP_TC_API_URL__|'"$REACT_APP_TC_API_URL"'|g' "$mainFile"
    sed -i 's|__REACT_APP_MSAL_CLIENT_ID__|'"$REACT_APP_MSAL_CLIENT_ID"'|g' "$mainFile"
    sed -i 's|__REACT_APP_MSAL_TENANT_ID__|'"$REACT_APP_MSAL_TENANT_ID"'|g' "$mainFile"
    sed -i 's|__REACT_APP_MSAL_SCOPE__|'"$REACT_APP_MSAL_SCOPE"'|g' "$mainFile"
    sed -i 's|__REACT_APP_VERSION__|'"$REACT_APP_VERSION"'|g' "$mainFile"

popd > /dev/null

# starting sshd process
sed -i "s/SSH_PORT/$SSH_PORT/g" /etc/ssh/sshd_config
/usr/sbin/sshd

# replace occurence of PORT in config site file for nginx
sed -i "s/PORT/$PORT/g" /etc/nginx/sites-available/tcsite
ln -s /etc/nginx/sites-available/tcsite /etc/nginx/sites-enabled/tcsite

echo "Restarting nginx..."
service nginx restart

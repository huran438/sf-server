#!/bin/sh

# Replace variables and start nginx
envsubst '$SERVER_NAME $BACKEND_URL' < /etc/nginx/templates/default.conf.template > /etc/nginx/conf.d/default.conf

exec nginx -g 'daemon off;'
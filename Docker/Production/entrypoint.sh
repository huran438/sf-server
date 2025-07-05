#!/bin/sh

# Make sure template is rendered with environment variables
envsubst '${API_URL} ${DASHBOARD_URL}' < /etc/nginx/templates/default.conf.template > /etc/nginx/conf.d/default.conf

# Start Nginx
exec nginx -g 'daemon off;'

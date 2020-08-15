FROM grafana/grafana
ENV GF_SECURITY_ADMIN_PASSWORD=changeit
ENV GF_USERS_ALLOW_SIGN_UP=false
USER 472
COPY provisioning /etc/grafana/provisioning
EXPOSE 3000


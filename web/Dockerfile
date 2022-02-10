FROM nginx

RUN mkdir -p /home/LogFiles /opt/startup /home/site/wwwroot \
    && echo "root:Docker!" | chpasswd \
    && echo "cd /home" >> /etc/bash.bashrc \
    && apt-get update \
    && apt-get install --yes --no-install-recommends \
    openssh-server \
    vim \
    curl \
    wget \
    tcptraceroute \
    openrc \
    yarn \
    net-tools \
    dnsutils \
    tcpdump \
    iproute2 \
    nano

ARG image_version=unknown
ENV REACT_APP_VERSION=$image_version
ENV TEAMCLOUD_IMAGE_VERSION=$image_version

# setup default site
RUN rm -f /etc/ssh/sshd_config
COPY deploy/startup /opt/startup
COPY build /home/site/wwwroot

# setup SSH
COPY deploy/sshd_config /etc/ssh/
RUN mkdir -p /home/LogFiles \
    && echo "root:Docker!" | chpasswd \
    && echo "cd /home" >> /root/.bashrc

RUN mkdir -p /var/run/sshd

RUN chmod -R +x /opt/startup

ENV PORT 8080
ENV SSH_PORT 2222
EXPOSE 2222 8080

ENV WEBSITE_ROLE_INSTANCE_ID localRoleInstance
ENV WEBSITE_INSTANCE_ID localInstance
ENV PATH ${PATH}:/home/site/wwwroot

WORKDIR /home/site/wwwroot

COPY deploy/tcsite /etc/nginx/sites-available/tcsite
COPY deploy/nginx.conf /etc/nginx/nginx.conf
RUN mkdir /etc/nginx/sites-enabled

ENTRYPOINT ["/opt/startup/init_container.sh"]
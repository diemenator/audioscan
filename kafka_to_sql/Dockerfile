FROM openjdk11
ENV KRB5_CONFIG=/kerberos-secrets/krb5.conf
RUN mkdir /tmp/event_consumer
WORKDIR /tmp/event_consumer
ARG zip_name=event_consumer-0.1
COPY target/universal/$zip_name.zip .
RUN unzip $zip_name.zip && mv /tmp/event_consumer/event_consumer-0.1 /opt/event_consumer && rm $zip_name.zip && rm -rf /tmp/event_consumer
EXPOSE 8558
WORKDIR /
CMD /opt/event_consumer/bin/event_consumer

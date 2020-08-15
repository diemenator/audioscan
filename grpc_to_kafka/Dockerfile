FROM openjdk11
RUN mkdir /tmp/grpc_to_kafka
WORKDIR /tmp/grpc_to_kafka
ARG zip_name=grpc_to_kafka-0.1
COPY target/universal/$zip_name.zip .
RUN unzip $zip_name.zip && mv /tmp/grpc_to_kafka/grpc_to_kafka-0.1 /opt/grpc_to_kafka && rm $zip_name.zip && rm -rf /tmp/grpc_to_kafka
EXPOSE 5003
WORKDIR /
CMD /opt/event_publisher_grpc/bin/grpc_to_kafka

FROM docker.elastic.co/logstash/logstash-oss:6.5.1

RUN logstash-plugin install logstash-input-kinesis && \
    logstash-plugin install logstash-output-elasticsearch && \
    logstash-plugin install logstash-codec-cloudwatch_logs

WORKDIR /usr/share/logstash/config
ADD ls-aws-cwl.conf ./
ADD entrypoint.sh ./
USER root
RUN chmod +x ./entrypoint.sh

# Create user and assign permission
CMD ["./entrypoint.sh"]
#!/bin/bash

EC2_AVAIL_ZONE=`curl -s http://169.254.169.254/latest/meta-data/placement/availability-zone`
EC2_REGION="`echo \"$EC2_AVAIL_ZONE\" | sed -e 's:\([0-9][0-9]*\)[a-z]*\$:\\1:'`"
PVT_IP=`curl -s http://169.254.169.254/latest/meta-data/local-ipv4`

echo Stream Name: $KINESIS_STREAM_NAME
echo Region: $EC2_REGION
echo Checkpoint DynamoDb Table: $KINESIS_CHECKPOINT
echo Elasticsearch Host: $ES_HOST
echo Docker Log Group Name: $DOCKER_LOG
echo =====================================
echo Region: $EC2_REGION

sed -i "s/{{aws-region}}/$EC2_REGION/g" ./ls-aws-log4net.conf
sed -i "s/{{stream-name}}/$KINESIS_STREAM_NAME/g" ./ls-aws-log4net.conf
sed -i "s/{{checkpoint-ddb}}/$KINESIS_CHECKPOINT/g" ./ls-aws-log4net.conf
sed -i "s/{{es-host}}/$ES_HOST/g" ./ls-aws-log4net.conf

logstash -f ./ls-aws-log4net.conf --verbose
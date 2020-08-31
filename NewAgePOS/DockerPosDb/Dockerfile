FROM mcr.microsoft.com/mssql/server:2019-CU6-ubuntu-16.04

USER root

RUN mkdir -p /usr/src/app
WORKDIR /usr/src/app

COPY NewAgePOS/DockerPosDb /usr/src/app

RUN chmod +x /usr/src/app/entrypoint.sh
RUN chmod +x /usr/src/app/create-db.sh

USER mssql
CMD /bin/bash ./entrypoint.sh
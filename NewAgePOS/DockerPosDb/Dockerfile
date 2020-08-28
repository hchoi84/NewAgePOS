FROM mcr.microsoft.com/mssql/server:latest

USER root

# Create app directory
RUN mkdir -p /usr/src/app
WORKDIR /usr/src/app

# Copy initialization scripts
COPY NewAgePOS/DockerPosDb /usr/src/app

# Grant permissions for the run-initialization script to be executable
RUN chmod +x /usr/src/app/entrypoint.sh
RUN chmod +x /usr/src/app/create-db.sh

# Run Microsoft SQl Server and initialization script (at the same time)
# Note: If you want to start MsSQL only (without initialization script) you can comment bellow line out, CMD entry from base image will be taken
USER mssql
CMD /bin/bash ./entrypoint.sh
FROM mcr.microsoft.com/mssql/server:2017-CU8-ubuntu

RUN mkdir /opt/init_test_db
COPY init_test_db.sql /opt/init_test_db
COPY init_test_db.sh /opt/init_test_db
ENV ACCEPT_EULA=Y
ENV SA_PASSWORD=Changeit(!)Changeit
CMD /opt/init_test_db/init_test_db.sh & /opt/mssql/bin/sqlservr

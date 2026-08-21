FROM eclipse-temurin:25

COPY ./target/back-end-project-0.0.1-SNAPSHOT.jar app.jar

ENTRYPOINT ["java", "-jar", "-agentlib:jdwp=transport=dt_socket,server=y,suspend=n,address=*:5005", "app.jar"]

EXPOSE 8080
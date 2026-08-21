package com.myorg;

import software.amazon.awscdk.*;
import software.amazon.awscdk.services.applicationautoscaling.EnableScalingProps;
import software.amazon.awscdk.services.ecs.*;
import software.amazon.awscdk.services.ecs.patterns.ApplicationLoadBalancedFargateService;
import software.amazon.awscdk.services.ecs.patterns.ApplicationLoadBalancedTaskImageOptions;
import software.amazon.awscdk.services.elasticloadbalancingv2.HealthCheck;
import software.amazon.awscdk.services.events.targets.SnsTopic;
import software.amazon.awscdk.services.iam.ManagedPolicy;
import software.amazon.awscdk.services.iam.Role;
import software.amazon.awscdk.services.iam.ServicePrincipal;
import software.amazon.awscdk.services.logs.LogGroup;
import software.constructs.Construct;

import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class ServiceStack extends Stack {

    public ServiceStack(final Construct scope, final String id, Cluster cluster, Role executionRole, Role taskRole, SnsTopic productEventsTopic) {
        this(scope, id, null, cluster, executionRole, taskRole, productEventsTopic);
    }


    public ServiceStack(final Construct scope, final String id, final StackProps props, Cluster cluster, Role executionRole, Role taskRole, SnsTopic productEventsTopic) {
        super(scope, id, props);

        Map<String, String> envVariables = new HashMap<>();
        envVariables.put("SPRING_DATASOURCE_URL", "jdbc:mysql://" + Fn.importValue("rds-endpoint")
                + ":3306/aws-project-01?createDatabaseIfNotExist=true");
        envVariables.put("SPRING_DATASOURCE_USERNAME", "admin");
        envVariables.put("SPRING_DATASOURCE_PASSWORD", Fn.importValue("rds-password"));
        envVariables.put("AWS_SNS_TOPIC_PRODUCT_EVENTS_ARN", productEventsTopic.getTopic().getTopicArn());

        LogGroup logGroup = LogGroup.Builder.create(this, "service-01-log-group")
                .logGroupName("/ecs/service-01")
                .removalPolicy(RemovalPolicy.DESTROY)
                .build();

        Role role = Role.Builder.create(this, "ecs-execution-role-01")
                .roleName("ecs-execution-role-01")
                .assumedBy(
                        new ServicePrincipal("ecs-tasks.amazonaws.com")
                )
                .managedPolicies(
                        List.of(
                                ManagedPolicy.fromAwsManagedPolicyName(
                                        "service-role/AmazonECSTaskExecutionRolePolicy"
                                )
                        )
                )
                .build();

        ApplicationLoadBalancedFargateService service = ApplicationLoadBalancedFargateService.Builder
                .create(this, "alb-01")
                .serviceName("service-01")
                .cluster(cluster)
                .cpu(512)
                .desiredCount(1)
                .listenerPort(8080)
                .memoryLimitMiB(1024)
                .taskImageOptions(
                        ApplicationLoadBalancedTaskImageOptions.builder()
                                .containerName("aws-project-01")
                                .image(ContainerImage.fromRegistry("jeftegoes/back-end-project-hub:1.0.0.q"))
                                .containerPort(8080)
//                                .executionRole(role)
//                                .taskRole(role)
                                .logDriver(LogDriver.awsLogs(AwsLogDriverProps.builder()
                                                .logGroup(logGroup)
                                                .streamPrefix("ecs")
                                                .build()
                                        )
                                )
                                .environment(envVariables)
                                .build()
                )
                .publicLoadBalancer(true)
                .assignPublicIp(true)
                .build();

        service.getTargetGroup().configureHealthCheck(new HealthCheck.Builder()
                .path("/actuator/health")
                .port("8080")
                .healthyHttpCodes("200")
                .build());

        ScalableTaskCount scalableTaskCount = service.getService().autoScaleTaskCount(EnableScalingProps.builder()
                .minCapacity(1)
                .maxCapacity(1)
                .build());

        scalableTaskCount.scaleOnCpuUtilization("service-01-auto-scaling", CpuUtilizationScalingProps.builder()
                .targetUtilizationPercent(50)
                .scaleInCooldown(Duration.seconds(60))
                .scaleOutCooldown(Duration.seconds(60))
                .build());

        productEventsTopic.getTopic().grantPublish(service.getTaskDefinition().getTaskRole());
    }
}
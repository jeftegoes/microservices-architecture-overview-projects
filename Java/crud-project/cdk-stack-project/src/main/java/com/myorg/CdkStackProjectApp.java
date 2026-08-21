package com.myorg;

import software.amazon.awscdk.App;

public class CdkStackProjectApp {
    public static void main(final String[] args) {
        App app = new App();

        VpcStack vpcStack = new VpcStack(app, "vpc-stack-project");

        ClusterStack clusterStack = new ClusterStack(app, "cluster-stack-project", vpcStack.getVpc());

        RdsStack rdsStack = new RdsStack(app, "rds-stack-project", vpcStack.getVpc());
        rdsStack.addStackDependency(vpcStack);

        SnsStack snsStack = new SnsStack(app, "sns-stack-project");

        ServiceStack serviceStack = new ServiceStack(app,
                "service-stack-project",
                clusterStack.getCluster(),
                null,
                null,
                snsStack.getProductEventsTopic());

        serviceStack.addStackDependency(clusterStack);
        serviceStack.addStackDependency(rdsStack);
        serviceStack.addStackDependency(snsStack);

        app.synth();
    }
}
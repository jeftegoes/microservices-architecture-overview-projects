package com.myorg;

import software.amazon.awscdk.App;

public class CdkStackProjectApp {
    public static void main(final String[] args) {
        App app = new App();

        VpcStack vpcStack = new VpcStack(app, "vpc-stack-project");
        ClusterStack clusterStack = new ClusterStack(app, "cluster-stack-project", vpcStack.getVpc());
        new ServiceStack(app, "service-stack-project", clusterStack.getCluster(), null, null);

        app.synth();
    }
}
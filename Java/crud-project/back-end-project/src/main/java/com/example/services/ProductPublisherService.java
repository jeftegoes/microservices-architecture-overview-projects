package com.example.services;

import com.example.enums.EventType;
import com.example.events.ProductEvent;
import com.example.models.Envelope;
import com.example.models.Product;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Qualifier;
import org.springframework.stereotype.Service;
import software.amazon.awssdk.services.sns.SnsClient;
import software.amazon.awssdk.services.sns.model.PublishRequest;
import software.amazon.awssdk.services.sns.model.PublishResponse;
import tools.jackson.databind.ObjectMapper;

@Service
public class ProductPublisherService {
    private static final Logger LOG = LoggerFactory.getLogger(ProductPublisherService.class);

    private SnsClient snsClient;
    private String productEventsTopic;
    private ObjectMapper objectMapper;

    public ProductPublisherService(SnsClient snsClient,
                                   @Qualifier("productEventsTopic") String productEventsTopic,
                                   ObjectMapper objectMapper) {
        this.snsClient = snsClient;
        this.productEventsTopic = productEventsTopic;
        this.objectMapper = objectMapper;
    }

    public void publishProductEvent(Product product, EventType eventType, String username) {
        ProductEvent productEvent = new ProductEvent();
        productEvent.setProductId(product.getId());
        productEvent.setCode(product.getCode());
        productEvent.setUsername(username);

        Envelope envelope = new Envelope();
        envelope.setEventType(eventType);
        envelope.setData(objectMapper.writeValueAsString(productEvent));

        PublishRequest request = PublishRequest.builder()
                .topicArn(productEventsTopic)
                .message(objectMapper.writeValueAsString(envelope))
                .build();

        PublishResponse response = snsClient.publish(request);
    }
}

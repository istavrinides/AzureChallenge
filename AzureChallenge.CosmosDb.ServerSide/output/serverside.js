function addRefund() {
    // capture the contextual variables we'll need
    var request = getContext().getRequest();
    var collection = getContext().getCollection();
    // run the core logic of the trigger
    addRefundImpl(request, collection);
}
function addRefundImpl(request, collection) {
    if (request.getOperationType() == "Delete") {
        // this is not a 'Create' or 'Replace' operation, so we can ignore it in this trigger
        return;
    }
    var document = request.getBody();
    if (document.type != "order" /* Order */) {
        // this is not an order, so we can ignore it in this trigger
        return;
    }
    var orderDocument = document;
    // check for any order items with a negative quantity
    var refundedProductIds = orderDocument.items.filter(i => i.quantity < 0).map(i => i.productId);
    if (refundedProductIds.length == 0) {
        return;
    }
    // prepare and upsert a refund document
    var refundDocument = {
        id: "refund-" + orderDocument.id,
        type: "refund" /* Refund */,
        customer: {
            id: orderDocument.customer.id
        },
        order: {
            id: orderDocument.id
        },
        refundedProducts: refundedProductIds
    };
    var inserted = collection.upsertDocument(collection.getSelfLink(), refundDocument);
    if (!inserted) {
        throw new Error("Could not insert refund document for order.");
    }
}

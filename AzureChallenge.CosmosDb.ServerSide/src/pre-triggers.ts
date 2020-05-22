function updateTournamentAggregate() {
    // capture the contextual variables we'll need
    var request = getContext().getRequest();
    var collection = getContext().getCollection();

    var document = request.getBody() as BaseDocument;

    // We only care about Tournament document types
    if (document.type != DocumentTypes.Tournament)
        return;

    var tournamentDoc = <TournamentDocument>document;

    if (request.getOperationType() === "Delete") {
        // this is not a 'Create' or 'Replace' operation, so we can ignore it in this trigger
        return;
    }
    else if (request.getOperationType() === "Upsert" || request.getOperationType() === "Replace") {

    }
    else if (request.getOperationType() === "Create") {

    }
    
    
    var orderDocument = <OrderDocument>document;

    // check for any order items with a negative quantity
    var refundedProductIds = orderDocument.items.filter(i => i.quantity < 0).map(i => i.productId);
    if (refundedProductIds.length == 0) {
        return;
    }

    // prepare and upsert a refund document
    var refundDocument: RefundDocument = {
        id: "refund-" + orderDocument.id,
        type: DocumentTypes.Refund,
        customer: {
            id: orderDocument.customer.id
        },
        order: {
            id: orderDocument.id
        },
        refundedProducts: refundedProductIds
    }
    var inserted = collection.upsertDocument(collection.getSelfLink(), refundDocument);
    if (!inserted) {
        throw new Error("Could not insert refund document for order.");
    }
}

const enum DocumentTypes {
    Tournament = "Tournament"
}

interface BaseDocument {
    id: string,
    type: DocumentTypes
}

interface TournamentDocument extends BaseDocument {
   public: boolean
}
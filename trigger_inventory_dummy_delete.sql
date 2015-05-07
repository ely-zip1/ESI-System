delimiter |

create trigger trigger_inventory_dummy_delete before delete on inventory_dummy
for each row
	begin
		insert into inventory_dummy_history values(null, old.id, old.warehouse_link, old.item_link, old.Location_link, old.cases, old.pieces, old.expiration_date, old.transaction_link, old.pricePerPiece, old.lineValue, old.price_selling_link, old.price_purchase_link, now(), "delete");
	end; |

delimiter ;
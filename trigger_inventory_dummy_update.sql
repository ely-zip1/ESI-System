delimiter |

create trigger trigger_inventory_dummy_update after update on inventory_dummy
for each row
	begin
		insert into inventory_dummy_history values(null, new.id, new.warehouse_link, new.item_link, new.Location_link, new.cases, new.pieces, new.expiration_date, new.transaction_link, new.pricePerPiece, new.lineValue, new.price_selling_link, new.price_purchase_link, now(), "update");
	end; |

delimiter ;
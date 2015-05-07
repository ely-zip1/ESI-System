delimiter |

create trigger trigger_transaction_entry before delete on transaction_entry
for each row 
	begin
		insert into transaction_entry_history values(null, old.entry_id, old.trans_type_link, old.doc_no, old.trans_date, old.source_WH_link, old.source_location_link, old.source_salesman_link, old.destination_WH_link, old.destination_location_link, old.destination_salesman_link, old.price_category, old.price_type, old.reason_code_link, old.comment, old.status, now(), "delete");
	end; |
	
delimiter ;
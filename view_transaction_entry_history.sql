create view view_transacion_entry_history as 
select e.history_id as history_id,
e.entry_id AS entry_id,
e.trans_no AS trans_no,
t.transaction_code AS transaction_code,
t.transaction_type AS transaction_type,
e.doc_no AS doc_no,
e.trans_date AS trans_date,
w.Name AS Source_Warehouse,
l.Code AS Source_Location,
e.source_salesman_link AS Source_Salesman,
ww.Name AS Destination_Warehouse,
ll.Code AS Destination_Location,
e.destination_salesman_link AS Destination_Salesman,
e.price_category AS price_category,
e.price_type AS price_type,
r.Reason_Description AS Reason,
r.Reason_Code AS reason_code,
e.comment AS Comment,
e.status AS status,
e.date as date,
e.event as event 

from ((((((esidb2.transaction_entry_history e 
left join esidb2.location l on((l.location_id = e.source_location_link))) 
left join esidb2.location ll on((ll.location_id = e.destination_location_link))) 
left join esidb2.reason_code r on((e.reason_code_link = r.reasoncode_id))) 
left join esidb2.warehouse w on((w.warehouse_id = e.source_WH_link))) 
left join esidb2.warehouse ww on((ww.warehouse_id = e.destination_WH_link))) 
left join esidb2.transaction_type t on((t.id = e.trans_type_link)))
CREATE OR REPLACE VIEW CDB_CARLIST_VIEW AS
SELECT 
    c.carId,
    c.brand_cd AS brand,
    c.model_cd AS model,
    c.year_num AS year,
    c.km_num AS km,
    c.price_num AS price,
    c.carStatus_ind AS status,
    c.record_tm AS listDate,
    u.userId AS ownerId,
    u.username_cd AS ownerUsername,
    u.userLocation_cd AS ownerLocation,
    img.imageUrl_cd AS thumbnailUrl
FROM CDB_LISTEDCAR c
JOIN CDB_REGISTEREDUSER u ON c.ownerId = u.userId
LEFT JOIN (
    SELECT car_id, imageUrl_cd
    FROM CDB_CARIMAGE
    WHERE (car_id, imageId) IN (
        SELECT car_id, MIN(imageId) FROM CDB_CARIMAGE GROUP BY car_id
    )
) img ON img.car_id = c.carId;

select * from CDB_CARLIST_VIEW;

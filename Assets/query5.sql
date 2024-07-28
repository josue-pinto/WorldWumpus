--INSERT INTO Pool (PlayerId)  SELECT  PlayerId from Chromosome group by PlayerId limit 170 ;

--INSERT INTO Selection (PlayerId,Steps)  SELECT PlayerId, count(playerid)  from CrossOver group by PlayerId";
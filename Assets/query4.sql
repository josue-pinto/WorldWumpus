--SELECT PlayerId from Pool GROUP by PlayerId
--INSERT INTO Pool (PlayerId, Gene , Weight )  SELECT  PlayerId,Gene , Weight from Chromosome WHERE PlayerId = (SELECT PlayerId from Chromosome group by PlayerId limit 170 )
--SELECT PlayerId, Gene  from Pool  WHERE PlayerId in ( SELECT PlayerId  from Pool)  order by 1
--DELETE  FROM Pool;
--DELETE FROM Chromosome;

SELECT PlayerId, Gene  from Pool   GROUP by Gene order by PlayerId, Id ASC limit 2

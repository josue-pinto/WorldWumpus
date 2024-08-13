--SELECT * from Chromosome where PlayerId = "Player4"
--SELECT PlayerId, sum(Weight) from Chromosome GROUP by PlayerId --where PlayerId = "Player4"
--INSERT INTO Selection (PlayerId,Steps)  SELECT PlayerId, sum(Weight) from Chromosome GROUP by PlayerId
--select *   from Selection where PlayerId In ("Player71","Player76") order by Fitness
--SELECT PlayerId FROM (select PlayerId, Fitness   from Selection  GROUP by PlayerId, Fitness ORDER by random() LIMIT 2 )  ORDER by Fitness ASC limit 1
--SELECT * FROM table ORDER BY RANDOM() LIMIT 1;
--SELECT * from Selection WHERE PlayerId in ("Player99","Player122")
SELECT * from Selection  where Fitness > 1
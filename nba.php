<?php
	require_once("Players.php");
	$name = $_REQUEST['name'];
	$callback = $_REQUEST['callback'];

	if ($callback != '') {
		
	}
	
try 
	{
		$conn = new PDO('mysql:host=info344.cxhxtck0pvvf.us-west-2.rds.amazonaws.com;dbname=info344', 'info344user', 'assignment1'); 
		$stmt = $conn->prepare('SELECT * FROM `TABLE 2` WHERE Name = "' . $name . '"');
		$stmt->execute();
		$results = $stmt->fetchAll();
		if (count($results)) {
			foreach ($results as $row)
			{
				$player = new Players($row['id'], $row['Name'], $row['GP'], $row['FGP'], $row['TPP'], $row['FTP'], $row['PPG']);
				$results = $player->getName() . ' ' .  $player->getGP() . ' ' .  $player->getFGP()  . ' ' .  $player->getTPP() . ' ' .  $player->getFTP(). ' ' . $player->getPPG() . '<br>';
				var_dump(player);
				echo $callback + "(" + json_encode($player) + ");";
			}
		}
		else 
		{
			
		}
	}
catch(PDOException $e)
{
	//echo 'ERROR: ' . $e->getMessage();
}

?>
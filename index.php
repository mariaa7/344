
<!DOCTYPE html>
<html>
<title>NBA</title>
<body>
<img src="index.jpg">
<form method="get">
Name: <input type="text" name="name">
<input type="submit">
</form>
<?php
	require_once("Players.php");

	$name = $_REQUEST['name'];

	if ($name != '') {
		echo 'Results for ' . $name . '<br>';
	}
	
try 
	{
		$conn = new PDO('mysql:host=info344.cxhxtck0pvvf.us-west-2.rds.amazonaws.com;dbname=info344', 'info344user', 'assignment1'); 
		$select = $conn->prepare('SELECT * FROM `TABLE 1` WHERE Name LIKE "%' . $name . '%"');
		$select->execute();
		$players = $select->fetchAll();
		if (count($players)) {
			foreach ($players as $p)
			{
				$player = new Players($p['id'], $p['Name'], $p['GP'], $p['FGP'], $p['TPP'], $p['FTP']);
				echo $player->getName() . ' ' .  $player->getGP() . ' ' .  $player->getFGP()  . ' ' .  $player->getTPP() . ' ' .  $player->getFTP(). '<br>';
			}
		}
		else 
		{
			echo "No results found!";
		}
	}
catch(PDOException $e)
{
	echo 'ERROR: ' . $e->getMessage();;
}

?>
</body>
</html>
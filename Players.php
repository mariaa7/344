<?php
class Players {
	private $id;
	private $Name;
	private $GP;
	private $FGP;
	private $TPP;
	private $FTP;

	function __construct($id, $Name, $GP, $FGP, $TPP, $FTP)
	{
		$this->id = $id;
		$this->Name = $Name;
		$this->GP = $GP;
		$this->FGP = $FGP;
		$this->TPP = $TPP;
		$this->FTP = $FTP;
	}
	function getID()
	{
		return $this->id;
	}
	function getName()
	{
		return $this->Name;
	}
	function getGP()
	{
		return $this->GP;
	}
	function getFGP()
	{
		return $this->FGP;
	}
	function getTPP()
	{
		return $this->TPP;
	}
	function getFTP()
	{
		return $this->FTP;
	}
}
?>
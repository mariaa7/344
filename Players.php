<?php
class Players {
	public $id;
	public $Name;
	public $GP;
	public $FGP;
	public $TPP;
	public $FTP;
	public $PPG;

	function __construct($id, $Name, $GP, $FGP, $TPP, $FTP, $PPG)
	{
		$this->id = $id;
		$this->Name = $Name;
		$this->GP = $GP;
		$this->FGP = $FGP;
		$this->TPP = $TPP;
		$this->FTP = $FTP;
		$this->PPG = $PPG;
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
	function getPPG()
	{
		return $this->PPG;
	}
}
?>
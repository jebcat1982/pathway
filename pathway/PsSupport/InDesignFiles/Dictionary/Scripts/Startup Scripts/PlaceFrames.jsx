﻿var columnRule=new Array("columns 1 Solid #808080");
var borderRule=new Array("@page-footnotes", ".5 solid #000000", "none", "none", "none", "@page-footnote", ".5 solid #000000", "none", "none", "none", "border", ".5 solid #808080", "none", "none", "none", "@page:first-footnotes", ".5 solid #000000", "none", "none", "none", "@page:first-footnote", ".5 solid #000000", "none", "none", "none");
var margin=new Array("");
var cropMarks = false;
var indexTab = false;
// --------------------------------------------------------------------------------------------
// <copyright file="PlaceFrames.jsx" from='2009' to='2010' company='SIL International'>
//      Copyright © 2009, SIL International. All Rights Reserved.   
//    
//      Distributable under the terms of either the Common Public License or the
//      GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright> 
// <author>Greg Trihus</author> <email>greg_trihus@sil.org</email>
// Created By:   James Prabu 
// Created On: Sep 10 2009   
// Modified By:  James Prabu                        
// Modified On:  June 01 2010
// Task Number : TD-1359(Apply macro from current page forward)
// <remarks> 
// changes in methods such as Group, UnGroup, TabIndex, PlaceFrames
// </remarks>
// --------------------------------------------------------------------------------------------

	
#target indesign
#targetengine "session"
#targetengine 'afterOpen'


var myDocument;
var myFrames=new Array();
var marginTop, marginBottom, marginLeft,marginRight, pageHeight, pageWidth, headerMinHeight = 6, dataMinHeight = 3, curPageNo =0,currentMarginTop;
var isBorderBottomExists, borderClass,times="";
var letterParagraphStyle, letterPushMargin=0, constantLetterParagraphStyle="letter_lethead_dicbody";
var activePageNumber=0;
//Delete Menu if it has been created already
	try
	{
		var my_main_menu = app.menus.item("$ID/Main"); 
		var my_type_menu = my_main_menu.menuElements.item("Macro"); 
		my_type_menu.remove(); 
	}
	catch(myError)
	{
	}

var mySampleScriptMenu = app.menus.item("$ID/Main").submenus.add("Macro");
var mySampleScriptAction = app.scriptMenuActions.add("Update Guide Words");
var mySampleScriptMenuItem = mySampleScriptMenu.menuItems.add(mySampleScriptAction);
var myEventListener = mySampleScriptAction.eventListeners.add("onInvoke",main);

mySampleScriptMenu = app.menus.item("$ID/Main").submenus.add("Macro");
mySampleScriptAction = app.scriptMenuActions.add("Update Guide Words - Partial");
mySampleScriptMenuItem = mySampleScriptMenu.menuItems.add(mySampleScriptAction);
myEventListener = mySampleScriptAction.eventListeners.add("onInvoke",partialMacro);

//Execute the DeleteEmptyPages() method on Document Open Event
(function() {

    app.eventListeners.add("afterOpen", afterOpen);

  function afterOpen(myEvent) 
  {
     if(app.documents[app.documents.length-1].masterSpreads.item(0).name == "F-FirstPage")
	var clicked=confirm("Headers are updated to contain the first and last word with the ''Update Guide Words'' tool bar item or Macro menu item.\nWould you like to update the headers now?")
	if (clicked==true)
	  {
		main();
	  }	  
  }

}())


function partialMacro()
{
	myDocument = app.documents[app.documents.length-1];
	try{ 
		activePageNumber=app.activeWindow.activePage.groups.item(0).parent.name;
		activePageNumber = activePageNumber - 1;
		//alert("Group " + activePageNumber);
		main();
	}
	catch(myError)
	{
		try{
			activePageNumber=app.activeWindow.activePage.textFrames.item(0).parent.name;
			activePageNumber = activePageNumber - 1;
			//alert("Text Frame " + activePageNumber);
			main();
		}
	    catch(myError)
		{
			if(activePageNumber==0)
			alert("Please select the valid page. Current Page should have atleast one TextFrame");
		}	
	}
}

function main()
{
	var d = new Date();

	times="Settings";
	times=times + "\n" + d;
	Settings();
	d = new Date();
	times=times + "\n" + d;
	
	times="\n" + times + "\nCheckBorder";
	times=times + "\n" + d;
	CheckBorder();
	d = new Date();	
	times=times + "\n" + d;

	times="\n" + times + "\nUngrouping";
	times=times + "\n" + d;
	Ungrouping();
	d = new Date();
	times=times + "\n" + d;
/*
	times="\n" + times + "\nMoveToFirstPage";
	times=times + "\n" + d;
	MoveToFirstPage();
	d = new Date();
	times=times + "\n" + d;
*/
	times="\n" + times + "\nPlaceFrames";
	times=times + "\n" + d;
	PlaceFrames();
	d = new Date();
	times=times + "\n" + d;

	times="\n" + times + "\nOptionals";
	times=times + "\n" + d;
	Optionals();
	d = new Date();
	times=times + "\n" + d;

	times="\n" + times + "\nGrouping";
	times=times + "\n" + d;
	Grouping();//Should be always as Last function
	d = new Date();
	times=times + "\n" + d;

	//alert(times);
}

//This method executes the function based on the Macro Variables
function Optionals()
{
	if(indexTab)	CreateIndexTab(); 
}

//This method containts basic settings of InDesign
function Settings()
{
	try
	{
		myDocument = app.documents[app.documents.length-1];
		curPageNo =0;
		myFrames=new Array();	
		myDocument.printPreferences.cropMarks=cropMarks;
		if(indexTab)	RemoveLockedFrames(); 
		if(margin.length == 5)
		{
			letterParagraphStyle = constantLetterParagraphStyle; //Always assigned Letter paragraph style for macro variable
			letterPushMargin = parseFloat(margin[3]);
		}
	}
	catch(myError)
	{

	}
}

//For Picture Caption
function ShowPicture()
{
	var myStory,pageHeight;
	//alert(myFrames.length);
	for(var myStoryCounter=activePageNumber; myStoryCounter <= myFrames.length-1;myStoryCounter++)
	{
			myStory = myFrames[myStoryCounter];
			 if(myStory.overflows)
			{
				pageHeight = myDocument.documentPreferences.pageHeight;
				frameBounds = myStory.geometricBounds;
				for(i=1;i<50;i++)
				{
						myStory.geometricBounds = [frameBounds[0], frameBounds[1], frameBounds[2] + (pageHeight * i) , frameBounds[3]];
						if(!myStory.overflows)
						break;
				}
			     //alert(i);
			}
		//alert("Frame " + myStoryCounter);
	}
}		

//This method moves frame one by one to all pages
function PlaceFrames()
{
	try
	{
	var d1 = new Date();
	
	times1="Header";
	times1=times1 + "\n" + d1;
	SetMasterPageForFirstPage();

	DeleteEmptyPages();
	//MoveToFirstPage();
	var myStory, myPage, frameBounds,pageBounds, frameType, frameHeight, minHeight, firstParagraphStyle;// = myDocument.stories.item(0); 
     var prePageNo=1;
	//headerHeight = 2.916666667;//2.083 + 0;//2.1; --> Removed
	
	myPage = myDocument.pages.item(0);
	
	marginLeft = myPage.marginPreferences.left;
	marginRight = myPage.marginPreferences.right;

	marginTop = myPage.marginPreferences.top;
	marginBottom =  myPage.marginPreferences.bottom;
	pageHeight = myDocument.documentPreferences.pageHeight - marginBottom;
     //alert(pageHeight);
     //alert(myDocument.documentPreferences.pageHeight + "\n" + myPage.marginPreferences.top + "\n" + myPage.marginPreferences.bottom);	
	pageWidth= myDocument.documentPreferences.pageWidth;
	currentMarginTop = marginTop;

    DrawPictureCaption();
	//return 0;
	d1 = new Date();
	times1=times1 + "\n" + d1;
	
	times1="\n" + times1 + "\nBody";
	times1=times1 + "\n" + d1;	
	//FitFrameToContent();

	curPageNo = activePageNumber;	
    
	for(var myStoryCounter=activePageNumber; myStoryCounter <= myFrames.length-1;myStoryCounter++)
	{
		goNewPage = false;
		myStory = myFrames[myStoryCounter];
		//alert(myStory.contents);
		frameType = GetFrameType(myStory);
		minHeight = GetMinHeight(frameType);
		firstParagraphStyle = GetFirstParagraphStyle(myStory)

		myStory.label = firstParagraphStyle;

		frameBounds = myStory.geometricBounds;
		frameHeight=frameBounds[2] - frameBounds[0];

		if(firstParagraphStyle == "pagebreaksinside" && (currentMarginTop + frameHeight) > pageHeight)
		{
			goNewPage = true;
		}
		if(currentMarginTop >= pageHeight -1 || (currentMarginTop + minHeight) > pageHeight  || 
																						firstParagraphStyle == "pagebreaksafter" || goNewPage == true)
		{
			//myDocument.pages.add();	
			AddNewPage(curPageNo + 1);
			curPageNo = curPageNo + 1;
			currentMarginTop = marginTop;
		}	
			
		MoveFrame(myStory, currentMarginTop, curPageNo);

		FitFrameToPage(myStory);
		
		frameBounds = myStory.geometricBounds;
		currentMarginTop = currentMarginTop + (frameBounds[2] - frameBounds[0]);

         //myStory.geometricBounds = [frameBounds[0], myPage.marginPreferences.left , frameBounds[2], pageWidth - myPage.marginPreferences.left]; //(pageWidth - marginLeft)
	     if(myStory.overflows &&   (pageHeight - currentMarginTop) > 3)
		 {
	 
			FitSingleFrameToContent(myStory);
		}
		else if(myStory.overflows)
		{
			myStory = SetOverflowsPages(myStory,frameBounds);
			curPageNo = myStory.parent.documentOffset;
			frameBounds = myStory.geometricBounds;
			currentMarginTop = marginTop + (frameBounds[2] - frameBounds[0]);
			//myStory.geometricBounds = [frameBounds[0], myPage.marginPreferences.left , frameBounds[2], pageWidth - myPage.marginPreferences.left]; //(pageWidth - marginLeft)
		}

		if(firstParagraphStyle == letterParagraphStyle)//"lethead_dicbody"
		{
			currentMarginTop += parseFloat(margin[3]);
		}

	}

	d1 = new Date();
	times1=times1 + "\n" + d1;
	
	times1="\n" + times1 + "\nFooter";
	times1=times1 + "\n" + d1;
	//alert("yes");
	//DeleteEmptyPages();

	
	d1 = new Date();
	times1=times1 + "\n" + d1;
	
		//alert(times1);
	}
	catch(myError)
	{
		//myDocument.pages.item(0).appliedMaster = null;
		//app.activeDocument.pages.item(0).appliedMaster = null;
	}
}

//This method assigns FirstPage Master spread to First Page only
function SetMasterPageForFirstPage()
{
	try
	{
				//app.documents[app.documents.length-1].masterSpreads.item("A-AllPage")
				myDocument.pages.item(0).appliedMaster = app.documents[app.documents.length-1].masterSpreads.item("F-FirstPage")
		//myDocument.pages.item(0).appliedMaster = app.activeDocument.masterSpreads.item("F-FirstPage");//F-FirstPage
		//app.activeDocument.pages.item(0).appliedMaster = app.activeDocument.masterSpreads.item("F-FirstPage");
	}
	catch(myError)
	{
		myDocument.pages.item(0).appliedMaster = null;
		//app.activeDocument.pages.item(0).appliedMaster = null;
	}
}	

//This method adds new page and assign Master Spread
function AddNewPage(pageNo)
{
	//Empty Page is already there, we need not add the new page
	if(pageNo != myDocument.pages.length)
	return;
	//alert(pageNo + "\t" + myDocument.pages.length);
	myDocument.pages.add();
	try
	{
		if(myDocument.documentPreferences.facingPages)
		{
			if((pageNo +1 ) % 2  == 0)
				myDocument.pages.item(pageNo).appliedMaster = app.documents[app.documents.length-1].masterSpreads.item("L-LeftPage");
				//myDocument.pages.item(pageNo).appliedMaster = app.activeDocument.masterSpreads.item("L-LeftPage");
				//app.activeDocument.pages.item(pageNo).appliedMaster = app.activeDocument.masterSpreads.item("L-LeftPage");
			else
				myDocument.pages.item(pageNo).appliedMaster = app.documents[app.documents.length-1].masterSpreads.item("R-RightPage");
				//myDocument.pages.item(pageNo).appliedMaster = app.activeDocument.masterSpreads.item("R-RightPage");
				//app.activeDocument.pages.item(pageNo).appliedMaster = app.activeDocument.masterSpreads.item("R-RightPage");
		}	
		else
		{
			myDocument.pages.item(pageNo).appliedMaster = app.documents[app.documents.length-1].masterSpreads.item("A-AllPage");
			//myDocument.pages.item(pageNo).appliedMaster = app.activeDocument.masterSpreads.item("A-AllPage");
			//app.activeDocument.pages.item(pageNo).appliedMaster = app.activeDocument.masterSpreads.item("A-AllPage");
		}
	}
	catch(myError)
	{
		myDocument.pages.item(pageNo).appliedMaster = null;
		//app.activeDocument.pages.item(pageNo).appliedMaster = null;
	}
}

//This method returns Paragraph style of the first paragraph of TextFrame
function GetFirstParagraphStyle(myStory)
{
	var elements;
	try
	{
		//elements = myStory.paragraphs[0].appliedParagraphStyle.name.toLowerCase().split("_");
		//alert(elements[elements.length-1]);
		//return elements[elements.length-1];
		return myStory.paragraphs[0].appliedParagraphStyle.name.toLowerCase();
	}
	catch(myError)
	{
		return "";
	}
}
//This method returns Required Minimum height based on the Frame Type
function GetMinHeight(frameType)
{
	if(frameType == "Header")
	{
		minHeight = headerMinHeight;
	}
	else
	{
		minHeight = dataMinHeight;
	}
	return minHeight;
}

//This method returns Frame type whether "Header" or "Data"
function GetFrameType(myStory)
{
	if(myStory.textFramePreferences.textColumnCount == 1 && myStory.contents.length < 5)
	{
		return "Header";
	}
	else
	{
		return "Data";
	}
}

//This method generates the liked frames for Parent frame
function SetOverflowsPages(myStory,frameBounds)
{

	var pageNo, pageCount, myTextFrame,frameWidth;
	frameWidth=frameBounds[3] - frameBounds[1];
	pageNo = myStory.parent.documentOffset;	
	while(myStory.overflows)
	{
		pageCount = myDocument.pages.length -1;
		
		if(pageCount >= pageNo)
		{
			//myDocument.pages.add();	
			AddNewPage(pageNo + 1);
			pageNo = pageNo + 1;
	
			myTextFrame = myDocument.textFrames.add();
			myTextFrame.label = myStory.label;
			//myTextFrame.fit(FitOptions.frameToContent);
			myTextFrame.previousTextFrame = myStory;
			
			myTextFrame.textFramePreferences.textColumnCount  = myStory.textFramePreferences.textColumnCount ;
			myTextFrame.textFramePreferences.textColumnGutter  = myStory.textFramePreferences.textColumnGutter;
			myTextFrame.textFramePreferences.verticalJustification  = myStory.textFramePreferences.verticalJustification;

			myTextFrame.geometricBounds = [marginTop, marginLeft, pageHeight, frameWidth + marginLeft]; //(pageWidth - marginLeft) 
			MoveFrame(myTextFrame, marginTop, pageNo);
		
			myStory=myTextFrame;
		}
	}
	if(!myStory.overflows)
	{
		BalancedColumns(myStory);
	}

	return myStory;
			
}

//Set Column Balance to Multi-Column Text Frames
function BalancedColumns(myStory)
{
		var fixedFrameBound, fitFrameBound;
		fixedFrameBound = myStory.geometricBounds; 
		for(unit=1;unit<=fixedFrameBound[2] * 2;unit++)
		{
			fitFrameBound = myStory.geometricBounds; 			
			myStory.geometricBounds=[fitFrameBound[0], fitFrameBound[1], fitFrameBound[2] - .5,fitFrameBound[3]];//.5 
			if(myStory.overflows)
			{
			myStory.geometricBounds=[fitFrameBound[0], fitFrameBound[1], fitFrameBound[2] + .5,fitFrameBound[3]];//.5
				break;
			}
		}
}

// This method move all frames to first page
function MoveToFirstPageAndCollectAllFrame()
{
	var myPage, myStory, myPageLength,arrayIndex =0,mystr,startIndex=0;
	//myDocument.pages.add();	
			
	for(var myPageCounter=activePageNumber; myPageCounter < myDocument.pages.length;myPageCounter++)
	{
		myPage = myDocument.pages.item(myPageCounter);//stories
		myPageLength = myPage.textFrames.length;
		startIndex=arrayIndex+1;
		for(var myStoryCounter=myPage.textFrames.length-1; myStoryCounter >= 0; myStoryCounter--)
		{
			myStory = myPage.textFrames.item(myStoryCounter);//stories
			//myFrames[arrayIndex] = myStory;
			arrayIndex = arrayIndex + 1;
			myStory.move(myDocument.pages[activePageNumber]);//myDocument.pages.length-1 
		}

	}
}

//This method call the ResetIndex()
function ReArrange()
{
	var myPage, myStory, myPageLength,arrayIndex =activePageNumber,mystr,startIndex=0;
	//myDocument.pages.add();	
			
	for(var myPageCounter=activePageNumber; myPageCounter < myDocument.pages.length;myPageCounter++)
	{
		myPage = myDocument.pages.item(myPageCounter);//stories
		myPageLength = myPage.textFrames.length;
		startIndex=arrayIndex+1;
		for(var myStoryCounter=myPage.textFrames.length-1; myStoryCounter >= 0; myStoryCounter--)
		{
			myStory = myPage.textFrames.item(myStoryCounter);//stories, 
			myStory.fit(FitOptions.frameToContent);
		
			myFrames[arrayIndex] = myStory;
			//alert(myFrames.length + "\n" + myFrames[arrayIndex].contents);
			arrayIndex = arrayIndex + 1;
		}
	    if(arrayIndex > startIndex)
		{
			ResetIndex(startIndex-1,arrayIndex-1);
			//alert("yes");
		}	
	}
}

//This method Re-arranges index in Array
function ResetIndex(startIndex,arrayIndex)
{
	try
	{
		//alert(startIndex + "\n" + arrayIndex)
		for(i=arrayIndex;i>=startIndex;i--)
		{
			for(j=startIndex;j<i;j++)
			{
				//alert("first " + myFrames[j].contents + "\n" + "last " + myFrames[j+1].contents);
				current = myFrames[j].geometricBounds;
				next = myFrames[j+1].geometricBounds;
				if(current[0]>next[0])
				{
					temp=myFrames[j];
					myFrames[j] =myFrames[j+1];
					myFrames[j+1]=temp;
				}
			}
		}
	}
	catch(myError)
	{
		return "  ";
	}
}
   
	   
// This method move all frames to first page
function CollectAllFrame()
{
	var myPage, myStory, myPageLength,arrayIndex =activePageNumber,mystr;
	//myDocument.pages.add();	
			
	for(var myPageCounter=activePageNumber; myPageCounter < myDocument.pages.length;myPageCounter++)
	{
		myPage = myDocument.pages.item(myPageCounter);//stories
		myPageLength = myPage.textFrames.length;

		for(var myStoryCounter=myPage.textFrames.length-1; myStoryCounter >= 0; myStoryCounter--)
		{
			myStory = myPage.textFrames.item(myStoryCounter);//stories
			myStory.fit(FitOptions.frameToContent);
		
			myFrames[arrayIndex] = myStory;
			arrayIndex = arrayIndex + 1;
		}
	}
}

//Move Frame to given position
function MoveFrame(myStory, currentMarginTop, pageNo)
{
	//alert(pageNo);
	
	myPage = myDocument.pages.item(pageNo);
	marginLeft = myPage.marginPreferences.left;
	marginRight = myPage.marginPreferences.right;
		
	myStory.move(myDocument.pages[pageNo]);	
	if(myDocument.documentPreferences.facingPages)
	{
		if((pageNo % 2) == 0 && pageNo > 0)//For Right Page and Not First page
		{
			myStory.move([marginLeft + pageWidth, currentMarginTop]);
		}
		else
		{
			//alert(marginTop, pageHeight);
			myStory.move([marginLeft, currentMarginTop]);
		}
	}
	else
	{
		myStory.move([marginLeft, currentMarginTop]);
	}
		
}

//This method fit the Frame to it's content size (in Height)
function FitFrameToContent()
{
	var myStory;
	for(var myStoryCounter=0; myStoryCounter <= myFrames.length-1;myStoryCounter++)
	{
		myStory = myFrames[myStoryCounter]; 
		var fitFrameBound;

		for(unit=1;unit<=pageHeight * 2;unit++)
		{
			fitFrameBound = myStory.geometricBounds; 	
			if(myStory.overflows)
			{
				myStory.geometricBounds=[fitFrameBound[0], fitFrameBound[1], fitFrameBound[2] + 1,fitFrameBound[3]];
				if(!myStory.overflows)
				{
					break;
				}				
			}
			else // if(!myStory.overflows) , Need to decreaseFrame Height
			{
				if(myStory.nextTextFrame == null)
				{
					myStory.geometricBounds=[fitFrameBound[0], fitFrameBound[1], fitFrameBound[2] - 2,fitFrameBound[3]];
					if(myStory.overflows)
					{
						myStory.geometricBounds=[fitFrameBound[0], fitFrameBound[1], fitFrameBound[2] + 1,fitFrameBound[3]];
						break;
					}
				}		
			}
		
		}
	
	}
}

//This method fit the Frame to it's content size (in Height)
function FitFrameToPage(myStory)
{
	try
	{
		myPage = myDocument.pages.item(curPageNo);
	
		marginLeft = myPage.marginPreferences.left;
		marginRight = myPage.marginPreferences.right;

		var fitFrameBound, frameHeight, myStoryHeight, frameLeft, frameWidth;
		fitFrameBound = myStory.geometricBounds; 
		myStoryHeight = fitFrameBound[2] -  fitFrameBound[0];


		if(myDocument.documentPreferences.facingPages)
		{
			if((curPageNo % 2) == 0 && curPageNo > 0)//For Right Page and Not First page
			{
				frameLeft = marginLeft + pageWidth;
				frameWidth = (pageWidth * 2) - marginRight;
			}
			else if((curPageNo % 2) == 0 && curPageNo == 0)//For Right Page and First page
			{
				frameLeft = marginLeft;
				frameWidth = pageWidth - marginRight;
				
			}
		    else
			{
				frameLeft = marginLeft;//For Mirror Page Left is Right
				frameWidth = pageWidth - marginRight;
			}
		}
		else
		{
				frameLeft = marginLeft;
				frameWidth = pageWidth - marginRight;		
		}
		//if(myStoryHeight >= pageHeight  || fitFrameBound[2]  >= pageHeight)
		/*if(fitFrameBound[2]  >= pageHeight)
		{
			frameHeight = pageHeight;
		}	
		else
		{
			 frameHeight = fitFrameBound[2];
		}*/
		frameHeight = pageHeight;
	    //alert("currentMarginTop " + currentMarginTop + " frameLeft " + frameLeft  + " frameHeight " + frameHeight   + " frameWidth " + frameWidth)
						//alert("current Top " + currentMarginTop + "\nPageHeight " + pageHeight);
		myStory.geometricBounds=[currentMarginTop,frameLeft , frameHeight ,frameWidth];
		 if(!myStory.overflows && myStory.nextTextFrame == null)
		{
			BalancedColumns(myStory);
		}
	}
	catch(myError)
	{
		alert(myError);
	}	
}

//This method fit the Frame to it's content size (in Height)
function FitFrameToPage100(myStory)
{
	try
	{
		var fitFrameBound, frameHeight, myStoryHeight, frameLeft, frameWidth;
		fitFrameBound = myStory.geometricBounds; 
		myStoryHeight = fitFrameBound[2] -  fitFrameBound[0];


		if(myDocument.documentPreferences.facingPages)
		{
			if((curPageNo % 2) == 0 && curPageNo > 0)//For Right Page and Not First page
			{
				frameLeft = marginLeft + pageWidth;
				frameWidth = (pageWidth * 2) - marginRight;
			}
			else if((curPageNo % 2) == 0 && curPageNo == 0)//For Right Page and First page
			{
				frameLeft = marginLeft;
				frameWidth = pageWidth - marginRight;
			}
		    else
			{
				frameLeft = marginRight;//For Mirror Page Left is Right
				frameWidth = pageWidth - marginLeft;
			}
		}
		else
		{
				frameLeft = marginLeft;
				frameWidth = pageWidth - marginRight;		
		}
		//if(myStoryHeight >= pageHeight  || fitFrameBound[2]  >= pageHeight)
		if(fitFrameBound[2]  >= pageHeight)
		{
			frameHeight = pageHeight;
		}	
		else
		{
			 frameHeight = fitFrameBound[2];
		}
	    //alert("currentMarginTop " + currentMarginTop + " frameLeft " + frameLeft  + " frameHeight " + frameHeight   + " frameWidth " + frameWidth)
		myStory.geometricBounds=[currentMarginTop,frameLeft , frameHeight ,frameWidth];
		 if(!myStory.overflows && myStory.nextTextFrame == null)
		{
			//alert(myStory.contents);
			BalancedColumns(myStory);
		}
	}
	catch(myError)
	{

	}	
}

//Delete the empty pages
function DeleteEmptyPages()
{
	//alert(app.documents.length);
		//alert(myDocument.pages.length);
	for(var j = myDocument.pages.length -1;j>=1;j--)
	{
		//alert(isEmptyPage(app.activeDocument.pages[j]), j);
		if(isEmptyPage(myDocument.pages[j]))
		{
			myDocument.pages[j].remove();
		}
		 else
		 {
			 break;
		 }
	}
}

//Check whether given page is empty
function isEmptyPage(page)
{
	var frames = page.textFrames;
	isEmptyPage = true;	
	for(var count = 0; count <= frames.length -1; count++)
	{
		if(frames[count].contents.length > 0)
		{
			isEmptyPage = false;
			return isEmptyPage;
		}
	}
	return isEmptyPage;
}

//This method makes grouping TextFrame with Lines all TextFrames
function Grouping()
{
		var myStory, firstParagraphStyle;
		var storyLength = myDocument.textFrames.length-1;		
		for(; storyLength >= 0 ; storyLength--)
		{
			myStory = myDocument.textFrames[storyLength];
			
			if(myStory.parent.name  > activePageNumber)
			{
				if(myStory.textFramePreferences.textColumnCount > 1)
				{
					makeGroup(myStory);
				}
			    //if(firstParagraphStyle.length > 0)
			   //alert(firstParagraphStyle + "   " + isBorderBottomExists + "   " + borderClass);
				if(isBorderBottomExists == true)// borderClass may be as "letHead"
				{
					firstParagraphStyle = GetFirstParagraphStyle(myStory);
					if(firstParagraphStyle == borderClass)
					{
						DrawBorderLine(myStory);
					}
				}
		    }

		}
}

//This method deletes Lines after UnGrouping
function DeleteLines()
{
		var myLine;
		var LineLength = myDocument.graphicLines.length -1;		
		for(; LineLength >= 0 ; LineLength--)
		{
			myLine = myDocument.graphicLines[LineLength];
			if(myLine.parent.name  > activePageNumber)
			myLine.remove();
		}	
}	

//This method makes Ungrouping TextFrame with Lines all TextFrames
function Ungrouping()
{
		var myGroup;
		var groupLength = myDocument.groups.length -1;	
		try
		{		
			if(groupLength >= 0)
			{
				for(; groupLength >= 0 ; groupLength--)
				{
					myGroup = myDocument.groups[groupLength];
					if(myGroup.parent.name  > activePageNumber)
					myGroup.ungroup();
				}	
				DeleteLines();
				//CollectAllFrame(); 
				ReArrange();
				MoveToFirstPageAndCollectAllFrame();
				//ReArrange();
				//FitFrameToContent();
			}
			else //Startup Event
			{
				//MoveToFirstPageAndCollectAllFrame();
				CollectAllFrame();
				ShowPicture();
			}
		}
		catch(myError)
		{
			//alert("U");
			//myLine.strokeWeight = "1pt";
		}		

}

//This method makes grouping Single TextFrame with Lines
function makeGroup(myStory)
{
var gb = myStory.geometricBounds;

var textWidth = gb[3] - gb[1];
var ptValue = 0.013888889;
var columnGutter = myStory.textFramePreferences.textColumnGutter;
var columnCount = myStory.textFramePreferences.textColumnCount;
var isPropertyAvailable = false;
var properties=new Array(3);
var strokeWeight, strokeStyle, strokeColor;

properties = GetPropertiesByStyle(myStory.label);

if(properties[0].length > 0)
{
	isPropertyAvailable = true;
}

textWidth = (textWidth - (columnGutter * (columnCount-1))) / columnCount;
var divideGutter = columnGutter /2;

var newWidth = gb[1];

for(var i=1;i<columnCount;i++)	
{
	myLine = myStory.parent.graphicLines.add();

	if(isPropertyAvailable == true)
	{
		SetProperties(myLine, properties, 1);
	}

	if(i==1)
	{
		newWidth = newWidth + textWidth + divideGutter; // Two Columns
		myLine.geometricBounds = [gb[0], newWidth, gb[2], newWidth];
		myStory.parent.groups.add ([myStory, myLine]);
	}
	else
	{
		newWidth = newWidth + textWidth + columnGutter + (ptValue/2); // > Two Columns
		myLine.geometricBounds = [gb[0], newWidth, gb[2], newWidth];
	}
}
}

//This method returns ColumnRule Properties based on given Style
function GetPropertiesByStyle(style)
{
	
	var properties =new Array();
	for(i=0;i<columnRule.length;i++)
	{
		properties = columnRule[i].split(" ");
		//alert(style + "   " + properties[0].toLowerCase());
		if(properties[0].toLowerCase() == style)//style = letdata
		{
			return properties;
		}	
	}
	properties[0] = "";
     return properties;
}	

//This method sets ColumnRule Properties
function SetProperties(myLine, properties, startIndex)
{
	//Block for Ruler Weight
	try
	{
		myLine.strokeWeight = properties[startIndex];
	}
	catch(myError)
	{
		myLine.strokeWeight = "5pt";
	}

	//Block for Ruler Style
	try
	{
		myLine.strokeType = properties[startIndex + 1];
	}
	catch(myError)
	{
		myLine.strokeType = "Solid";
	}

	//Block for Ruler Color
	try
	{
		myLine.strokeColor = properties[startIndex + 2];
		//myLine.fillColor = properties[3];
		//myLine.underlineColor = properties[3];
	}
	catch(myError)
	{
		myLine.strokeColor = "Cyan";
		//myLine.fillColor = "Cyan";
		//myLine.underlineColor = "Cyan";
	}

}	

//This method draws lines based on border styles for Top, Bottom, Left and Right
function DrawBorderLine(myStory)
{
	var isGrouped = false; 
	var gb = myStory.geometricBounds;
	 if(borderRule[1] != "none") // Top
	 {
		 ApplyBorderProperties(myStory, 1, gb[0], gb[1], gb[0], gb[3], isGrouped)
		 isGrouped = true;
	 }
	 if(borderRule[2] != "none") // Right
	 {
		 ApplyBorderProperties(myStory, 2, gb[0], gb[3], gb[2], gb[3], isGrouped)
		 isGrouped = true;
	 } 
	 if(borderRule[3] != "none") // Bottom
	 {
		 ApplyBorderProperties(myStory, 3, gb[2], gb[1], gb[2], gb[3], isGrouped)
		 isGrouped = true;
	 }	 
	 if(borderRule[4] != "none") // Left
	 {
		ApplyBorderProperties(myStory, 4, gb[0], gb[1], gb[2], gb[1], isGrouped)
		isGrouped = true;
	 }	
}

//This method applies border styles to Line and Make group with Parent
function ApplyBorderProperties(myStory, borderIndex, top, left, bottom, right, isGrouped)
{
	try
	{

	myLine = myStory.parent.graphicLines.add();
	properties = borderRule[borderIndex].split(" ");
	SetProperties(myLine, properties, 0);
	myLine.geometricBounds = [top, left, bottom, right]; // Bottom
	if(isGrouped==false)
	myStory.parent.groups.add ([myStory, myLine]);
	}
     catch(myError)
	 {
	 }
}

//This method checks whether any Border Styles(Top,Bottom,Left,Right) exits for letHead
function CheckBorder()
{
	//letter_letHead_dicBody
	borderClass = "letter_" + borderRule[0].toLowerCase() + "_dicbody";
	if((borderRule[1] == "none" && borderRule[2] == "none"  && borderRule[3] == "none" && borderRule[4] == "none") || borderRule.length == 1 )
		isBorderBottomExists = false;
	else
		isBorderBottomExists = true;
}

//This method sets the caption for Picture
function DrawPictureCaption()
{
	var gbPicture, gbContainer, gbCaption, pictureHeight, captionHeight, parentHeight;
	var pictures = myDocument.allGraphics;
try
{
	//alert(pictures.length);
	for(var count = 0; count <= pictures.length; count++)
	{
		picture = pictures[count];
		if(picture.parent.name  > activePageNumber)
		{
			if(picture.parent.allPageItems.length > 0)
			{
				caption = picture.parent.textFrames[0];
				
				if(caption.overflows)
				{
					gbPicture = picture.geometricBounds;
					gbContainer = picture.parent.geometricBounds;
					 pictureHeight = gbPicture[2] - gbPicture[0];
					caption.geometricBounds = [gbPicture[2] + 0.1, gbPicture[1], gbPicture[2] + 1, gbPicture[3]];
					FitSingleFrameToContent(caption);
					gbCaption = caption.geometricBounds;
					captionHeight = gbCaption[2] - gbCaption[0];
					parentHeight = (pictureHeight + captionHeight);// + 0.5;//captionHeightUnit
					if(gbContainer[1]==gbContainer[3])
					gbContainer[3] += 0.5;
					//alert(gbContainer[0] + "\n" + gbContainer[1] + "\n" +  gbContainer[0] + parentHeight + "\n" +  gbContainer[3])
					picture.parent.geometricBounds = [gbContainer[0], gbContainer[1], gbContainer[0] + parentHeight , gbContainer[3]];
				}
			 }
		}
     }
     //alert("End");
}
 catch(myError)
 {
	 //alert(myError);
 }
}

//This method reduces the Text Frame height to content
function FitSingleFrameToContent(myStory)
{
	try
	{	
		var fixedFrameBound, fitFrameBound;
		fixedFrameBound = myStory.geometricBounds; 
		//alert(myStory.contents)
		for(unit=1;unit<=fixedFrameBound[2] * 2;unit++)
		{
			fitFrameBound = myStory.geometricBounds; 			
			myStory.geometricBounds=[fitFrameBound[0], fitFrameBound[1], fitFrameBound[2] + 1 ,fitFrameBound[3]];//.5
			if(!myStory.overflows)
			{
				break;
			}
		}
	}
     catch(myError)
	 {
	 }
}


//This method creates the IndexTab
function CreateIndexTab()
{
	var myTop, myLeft, myWidth, myHeight, myTextFrame, height=1.5, firstLetter, lastLetter, prePageLetter;
	myPage = myDocument.pages.item(0);
	marginLeft = myPage.marginPreferences.left;
	pageWidth= myDocument.documentPreferences.pageWidth * 2;
	myTop = myPage.marginPreferences.top;
	firstLetter = "  ";
	lastLetter = "  ";
	for(var myPageCounter=activePageNumber; myPageCounter < myDocument.pages.length;myPageCounter++)
	{
		myPage = myDocument.pages.item(myPageCounter);
		myTextFrame = myDocument.textFrames.add();
	
		myTextFrame.move(myDocument.pages[myPageCounter]);	
		firstLetter = GetFirstParagraphLetter(myPage);//"  A";
		if(firstLetter == "  ") 
			firstLetter=lastLetter;	

		myTextFrame.contents = firstLetter;
		lastLetter = GetLastParagraphLetter(myPage);
	
		if(lastLetter == "  ") 
			lastLetter=firstLetter;		
		
		myTextFrame.textFramePreferences.verticalJustification=Justification.centerAlign;

	    //Set geometricBounds
		if(myPageCounter > 0 && prePageLetter != firstLetter)
			myTop = myTop + 1.5;
			
		myHeight = myTop + 1.5;
		
		myLeft = .5;
		if(myDocument.documentPreferences.facingPages)
		{
			if((myPageCounter % 2) == 0 && myPageCounter > 0)//For Right Page and Not First page
				myLeft = pageWidth - 2;			
		}
		myWidth = myLeft + 1.5;	
		
		myTextFrame.geometricBounds = [myTop, myLeft, myHeight, myWidth]; 
		myTextFrame.locked = true;

		//myTextFrame.paragraphs[0].words[0].appliedCharacterStyle ="finger.-current_locator_letHead_dicBody";		
		myTextFrame.fillColor="Black";
		
		prePageLetter = firstLetter;
	}
}

// This method removes all the Locked Frames
function RemoveLockedFrames()
{
	var myPage, myStory, myPageLength,arrayIndex =0,mystr;
			
	for(var myPageCounter=0; myPageCounter < myDocument.pages.length;myPageCounter++)
	{
		myPage = myDocument.pages.item(myPageCounter);//stories
		myPageLength = myPage.textFrames.length;

		for(var myStoryCounter=myPage.textFrames.length-1; myStoryCounter >= 0; myStoryCounter--)
		{
			myStory = myPage.textFrames.item(myStoryCounter);//stories
			if(myStory.locked)
			{
				myStory.locked = false;
				myStory.remove();
			}
		}
	}
}

//This method returns First occurance of the Letter 
function GetFirstParagraphLetter(myPage)
{
	var result="  ";
	try
	{
		for(var myStoryCounter=myPage.textFrames.length-1; myStoryCounter >= 0; myStoryCounter--)
		 {
			 myStory = myPage.textFrames.item(myStoryCounter);//stories
			if(myStory.paragraphs[0].appliedParagraphStyle.name.toLowerCase() == "letter_lethead_dicbody")
			{
				result = "  " + myStory.contents.substring(0,1).toUpperCase();
				break;
			}	
		 }
	
		return result;

	}
	catch(myError)
	{
		return "  ";
	}
}

//This method returns Last occurance of the Letter 
function GetLastParagraphLetter(myPage)
{
	var result="  ";
	try
	{
		for(var myStoryCounter=myPage.textFrames.length-1; myStoryCounter >= 0; myStoryCounter--)
		 {
			 myStory = myPage.textFrames.item(myStoryCounter);//stories
			if(myStory.paragraphs[0].appliedParagraphStyle.name.toLowerCase() == "letter_lethead_dicbody")
			result = "  " + myStory.contents.substring(0,1).toUpperCase();
		 }
		return result;
	}
	catch(myError)
	{
		return "  ";
	}
}
























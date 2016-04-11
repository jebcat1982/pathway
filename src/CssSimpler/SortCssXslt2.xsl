﻿<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    version="1.0">

    <xsl:output method="xml" encoding="utf-8"/>
    
    <!-- Copy nodes -->
    <xsl:template match="node()|@*">
        <xsl:copy>
            <xsl:apply-templates select="node()|@*"/>
        </xsl:copy>
    </xsl:template>
    
    <xsl:template match="xsl:stylesheet">
        <xsl:copy>
            <xsl:apply-templates select="@*"/>
            <xsl:apply-templates select="xsl:template">
                <xsl:sort select="*/*[@name='class']" order="ascending"/>
            </xsl:apply-templates>
        </xsl:copy>
    </xsl:template>
    
</xsl:stylesheet>
<%@ include file="/include.jsp"%>
<%@ taglib prefix="forms" tagdir="/WEB-INF/tags/forms" %>
<%@ taglib prefix="props" tagdir="/WEB-INF/tags/props" %>
<%@ taglib prefix="l" tagdir="/WEB-INF/tags/layout" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>

<jsp:useBean id="propertiesBean" scope="request" type="jetbrains.buildServer.controllers.BasePropertiesBean"/>

<div class="parameter">
  MSBuild Version: <props:displayValue name="prop.msbuildVersion" showInPopup="${false}" emptyValue="<none>"/>
</div>

<div class="parameter">
  MSBuild ToolsVersion: <props:displayValue name="prop.msbuildToolsVersion" showInPopup="${false}" emptyValue="<none>"/>
</div>

<div class="parameter">
  Virtual Project File: <props:displayValue name="prop.virtualProjectRoot" showInPopup="${false}" emptyValue="<none>"/>
</div>

<div class="parameter">
  Output File: <props:displayValue name="prop.outputFile" showInPopup="${false}" emptyValue="<none>"/>
</div>

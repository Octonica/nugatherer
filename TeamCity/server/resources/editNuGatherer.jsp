<%@ taglib prefix="props" tagdir="/WEB-INF/tags/props" %>
<%@ taglib prefix="l" tagdir="/WEB-INF/tags/layout" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<%@ taglib prefix="forms" tagdir="/WEB-INF/tags/forms" %>

<jsp:useBean id="propertiesBean" scope="request" type="jetbrains.buildServer.controllers.BasePropertiesBean"/>
<jsp:useBean id="nuGatherer" scope="application" class="octonica.nugatherer.common.NuGathererProperties"/>

<tr>
  <th>
    <label for="prop.msbuildVersion">MSBuild Version: </label>
  </th>
  <td>
    <props:selectProperty name="prop.msbuildVersion" style="width:30em">
      <c:forEach var="it" items="${nuGatherer.msbuildVersions}">
        <props:option value="${it}"><c:out value="${it}"/></props:option>
      </c:forEach>
    </props:selectProperty>
    <span class="error" id="error_prop.msbuildVersion"></span>
  </td>
</tr>

<tr>
  <th>
    <label for="prop.msbuildToolsVersion">MSBuild ToolsVersion: </label>
  </th>
  <td>
    <props:selectProperty name="prop.msbuildToolsVersion" style="width:30em">
      <c:forEach var="it" items="${nuGatherer.msbuildToolsVersions}">
        <props:option value="${it}"><c:out value="${it}"/></props:option>
      </c:forEach>
    </props:selectProperty>
    <span class="error" id="error_prop.msbuildToolsVersion"></span>
  </td>
</tr>

<tr>
  <th>
    <label for="prop.projectFile">Virtual Project File: </label>
  </th>
  <td>
    <props:textProperty name="prop.virtualProjectRoot" style="width:30em"/>
    <span class="smallNote">
        Required. Specify the path to the MSBuild file which contains references to projects (*.csproj).
    </span>
    <span class="error" id="error_prop.virtualProjectRoot"></span>
  </td>
</tr>

<tr>
  <th>
    <label for="prop.outputFile">Output File: </label>
  </th>
  <td>
    <props:textProperty name="prop.outputFile" style="width:30em"/>
    <span class="smallNote">
        Optional. Specify to generate consolidated NuGet package file (packages.config).
    </span>
    <span class="error" id="error_prop.outputFile"></span>
  </td>
</tr>
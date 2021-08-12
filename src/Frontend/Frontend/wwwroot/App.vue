<template>
	<div class="main-container">
		<router-view></router-view>
	</div>
</template>

<script>
export default {
	name: `AppComponent`
};
</script>

<style>
.main-container {
	display: flex;
	flex-direction: row;
	align-items: center;
	justify-content: center;
}
</style>
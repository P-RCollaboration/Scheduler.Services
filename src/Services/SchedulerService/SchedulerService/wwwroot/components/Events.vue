<template>
	<div class="events-container">
		<span>You have events:</span>
		<div v-for="(item, index) in events" :key="index">
			<span>
				Event {{ item.name }} at {{ item.date }}
			</span>
			<span>
				{{ item.description }}
			</span>
		</div>
	</div>
</template>

<script>
	export default {
		name: `AuthForm`,
		data() {
			return {
				events: []
			}
		},
		async created() {
			// In real application address to service can be closed Nginx or another proxy server, but in this example, I stay all thing easiest as it can.
			const response = await fetch(`http://localhost:5007/api/event/latest`);
			this.events = await response.json();
		}
	}
</script>

<style>
	.events-container {
		min-height: 350px;
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
	}

	.events-container > div {
		width: 400px;
		margin-top: 20px;
		font-size: 15px;
		font-weight: bold;
		background-color: #f2f2f2;
		display: flex;
		flex-direction: column;
	}
</style>
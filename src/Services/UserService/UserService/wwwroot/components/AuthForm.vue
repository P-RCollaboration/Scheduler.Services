<template>
	<div class="authform-container">
		<span>Login</span>
		<input type="text" required v-model="userLogin" />
		<span>Password</span>
		<input type="text" required v-model="password" />
		<button @click="login()">Sign in</button>
	</div>
</template>

<script>
export default {
	name: `AuthForm`,
	data() {
		return {
			userLogin: ``,
			password: ``
		}
	},
	methods: {
		async login() {
			// In real application address to service can be closed Nginx or another proxy server, but in this example, I stay all thing easiest as it can.
			const response = await fetch(`http://localhost:5008/api/user?login=${this.userLogin}&password=${this.password}`);

			//some actions with response ....

			this.$router.push({ path: "/events" });
		}
	}
}
</script>

<style>
.authform-container {
	width: 450px;
	height: 350px;
	background-color: #f2f2f2;
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
}
.authform-container > button {
	margin-top: 20px;
	font-size: 15px;
	font-weight: bold;
}
</style>